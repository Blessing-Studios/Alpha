using Blessing.Core.GameEventSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [Serializable]
    public struct InventoryItemData : IEquatable<InventoryItemData>, INetworkSerializeByMemcpy
    {
        public Guid Id;
        public int ItemId;
        public Vector2Int Position;
        public bool Rotated;

        public bool Equals(InventoryItemData other)
        {
            return
                (this.Id == other.Id) &&
                (this.ItemId == other.ItemId) &&
                (this.Position == other.Position) &&
                (this.Rotated == other.Rotated);
        }
    }
    public class Inventory : NetworkBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public InventoryItem Owner; // Para testar
        [field: SerializeField] public string Name { get; private set; }
        public int Width = 20;
        public int Height = 10;

        public NetworkVariable<Vector2Int> GridSize = new NetworkVariable<Vector2Int>
            (
                new Vector2Int(0, 0),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner
            );

        // [field: SerializeField]
        // public NetworkVariable<bool> IsInitialized = new NetworkVariable<bool>
        //     (
        //         false,
        //         NetworkVariableReadPermission.Everyone,
        //         NetworkVariableWritePermission.Owner
        //     );

        private bool isItemsInitialized = false;

        [field: SerializeField] public NetworkVariable<InventoryItemData> OwnerData = new(new InventoryItemData(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public InventoryGrid InventoryGrid;
        public List<InventoryItem> ItemList = new();
        public NetworkList<InventoryItemData> InventoryNetworkList;
        public List<InventoryItemData> InventoryLocalList;
        public InventoryItem[,] ItemSlot;

        [Header("Events")]
        public GameEvent OnAddItem;
        public GameEvent OnRemoveItem;

        [Header("Debug")]
        public int ItemSlotX;
        public int ItemSlotY;
        // public List<GameEventListener> Listeners { get; set; }

        protected virtual void Awake()
        {
            InventoryNetworkList = new NetworkList<InventoryItemData>
                (
                    new List<InventoryItemData>(),
                    NetworkVariableReadPermission.Everyone,
                    NetworkVariableWritePermission.Owner
                );
        }
        // Initialize Inventory, Where inventoryItem is the item containing this inventory
        public void SetNetworkVariables(int gridWidth, int gridHeight, InventoryItem inventoryItem = null)
        {
            if (!HasAuthority) return;

            Width = gridWidth;
            Height = gridHeight;
            GridSize.Value = new Vector2Int(Width, Height);
            ItemSlot = new InventoryItem[Width, Height];

            if (inventoryItem == null) return;

            gameObject.name = inventoryItem.Item.name + "-Container";
            inventoryItem.Inventory = this;
            Owner = inventoryItem;
            OwnerData.Value = inventoryItem.Data;
        }
        public virtual void Initialize()
        {
            Width = GridSize.Value.x;
            Height = GridSize.Value.y;

            ItemSlot = new InventoryItem[Width, Height];

            InventoryItem inventoryItem = FindItem(OwnerData.Value);

            UpdateLocalList(ref InventoryLocalList, InventoryNetworkList);

            if (!isItemsInitialized)
            {
                InitializeItems();
            }

            if (inventoryItem == null) return;

            gameObject.name = inventoryItem.Item.name + "-Container";

            inventoryItem.Inventory = this;
            Owner = inventoryItem;
        }

        protected virtual void InitializeItems()
        {
            RemoveAllInventoryItems();

            foreach (InventoryItemData itemData in InventoryNetworkList)
            {
                InventoryItem inventoryItem = FindItem(itemData);
                inventoryItem.Data = itemData;
                AddInventoryItem(inventoryItem, inventoryItem.Data.Position);
            }

            isItemsInitialized = true;
        }

        public override void OnNetworkSpawn()
        {
            GridSize.OnValueChanged += OnGridSizeValueChanged;
            OwnerData.OnValueChanged += OnOwnerDataOnValueChanged;
            InventoryNetworkList.OnListChanged += OnInventoryNetworkListChanged;

            // This is for a player that enters after this inventory is already on scene by another client
            if (!HasAuthority)
                Initialize();
        }

        private void OnGridSizeValueChanged(Vector2Int previousValue, Vector2Int newValue)
        {
            if (HasAuthority) return;

            Width = newValue.x;
            Height = newValue.y;

            ItemSlot = new InventoryItem[Width, Height];
        }

        private void OnOwnerDataOnValueChanged(InventoryItemData previousValue, InventoryItemData newValue)
        {
            if (HasAuthority) return;

            InventoryItem inventoryItem = FindItem(newValue);

            if (inventoryItem == null) return;

            gameObject.name = inventoryItem.Item.name + "-Container";

            inventoryItem.Inventory = this;
            Owner = inventoryItem;
        }
        private void OnInventoryNetworkListChanged(NetworkListEvent<InventoryItemData> changeEvent)
        {
            // Link com mais informações em como usar o NetworkListEvent 
            // https://discussions.unity.com/t/how-to-use-networklist/947471/2\

            if (!UpdateLocalList(ref InventoryLocalList, InventoryNetworkList)) return;

            // TODO: fazer uma lógica que funciona caso mais de um item tenha mudado
            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Add)
            {
                InventoryItem inventoryItem = FindItem(changeEvent.Value);
                inventoryItem.Data = changeEvent.Value;
                AddInventoryItem(inventoryItem, inventoryItem.Data.Position);
                UpdateFromInventory();
            }

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Remove)
            {
                for (int i = 0; i < ItemList.Count; i++)
                {
                    InventoryItem item = ItemList[i];
                    if (item.Data.Id == changeEvent.Value.Id)
                    {
                        item.Data = changeEvent.Value;
                        RemoveInventoryItem(item);
                        UpdateFromInventory();
                        break;
                    }
                }
            }
        }

        protected virtual void Start()
        {
            if (InventoryGrid == null)
            {
                // TODO: temporário
                InventoryGrid = GameManager.Singleton.InventoryController.OtherInventoryGrid;
            }
        }
        protected virtual void Update()
        {
            if (ShowDebug)
            {
                ItemSlotX = ItemSlot.GetLength(0);
                ItemSlotY = ItemSlot.GetLength(1);
            }
        }

        protected InventoryItem FindItem(InventoryItemData data)
        {
            return GameManager.Singleton.FindInventoryItem(data);
        }

        protected bool UpdateLocalList(ref List<InventoryItemData> localList, NetworkList<InventoryItemData> networkList)
        {
            return GameManager.Singleton.UpdateLocalList(ref localList, networkList);
        }
        public bool AddItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (!AddInventoryItem(inventoryItem, position)) return false;

            // InventoryNetworkList has to be changed after InventoryLocalList
            InventoryLocalList.Add(inventoryItem.GetData());
            InventoryNetworkList.Add(inventoryItem.GetData());

            if (OnAddItem != null)
                OnAddItem.Raise(this, inventoryItem);

            return true;
        }

        private bool AddInventoryItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (inventoryItem == null) return false;

            // If item is already in inventory, return false
            if (ItemList.Contains(inventoryItem))
            {
                Debug.LogWarning("AddItem Failed");
                return false;
            }

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Criar um id ou hash no lugar de salvar o inventoryItem ref
                    ItemSlot[position.x + x, position.y + y] = inventoryItem;
                }
            }

            ItemList.Add(inventoryItem);

            return true;
        }

        public bool RemoveItem(InventoryItem inventoryItem)
        {
            // IF item is not in the inventory, return false
            if (!RemoveInventoryItem(inventoryItem)) return false;

            // InventoryNetworkList has to be changed after InventoryLocalList
            InventoryLocalList.Remove(inventoryItem.GetData());
            InventoryNetworkList.Remove(inventoryItem.GetData());

            if (OnRemoveItem != null)
                OnRemoveItem.Raise(this, inventoryItem);

            return true;
        }

        private void RemoveAllInventoryItems()
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                RemoveInventoryItem(ItemList[i]);
            }
        }
        private bool RemoveInventoryItem(InventoryItem inventoryItem)
        {
            if (inventoryItem == null) return false;

            // IF item is not in the inventory, return false
            if (!ItemList.Contains(inventoryItem)) return false;

            for (int x = 0; x < inventoryItem.Width; x++)
            {
                for (int y = 0; y < inventoryItem.Height; y++)
                {
                    ItemSlot[inventoryItem.Data.Position.x + x, inventoryItem.Data.Position.y + y] = null;
                }
            }

            ItemList.Remove(inventoryItem);

            return true;
        }

        private void UpdateFromInventory()
        {
            if (InventoryGrid != null)
                InventoryGrid.UpdateFromInventory();
        }

        public void GetOwnership()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            GameManager.Singleton.GetOwnership(networkObject);

            // ulong LocalClientId = NetworkManager.Singleton.LocalClientId;
            // if (LocalClientId != OwnerClientId)
            //     ChangeOwnership(LocalClientId);
        }

        public void ChangeOwnership(ulong id)
        {
            GetComponent<NetworkObject>().ChangeOwnership(id);
        }

        public bool CheckAvailableSpace(Vector2Int position, int width, int height)
        {

            if (!BoundaryCheck(position, width, height))
            {
                return false;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (ItemSlot[position.x + x, position.y + y] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool BoundaryCheck(Vector2Int position, int width, int height)
        {
            if (!PositionCheck(position))
            {
                return false;
            }

            position.x += width - 1;
            position.y += height - 1;

            if (!PositionCheck(position))
            {
                return false;
            }

            return true;
        }

        public bool PositionCheck(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0) return false;

            if (position.x >= Width || position.y >= Height) return false;

            return true;
        }
    }
}

