using Blessing.Core.GameEventSystem;
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
        [field: SerializeField] public string Name { get; private set; }
        public int Width = 20;
        public int Height = 10;
        public InventoryGrid InventoryGrid;
        public List<InventoryItem> ItemList = new();
        public NetworkList<InventoryItemData> InventoryNetworkList;
        public List<InventoryItemData> InventoryLocalList;
        public InventoryItem[,] ItemSlot;

        [Header("Events")]
        public GameEvent OnAddItem;
        public GameEvent OnRemoveItem;
        // public List<GameEventListener> Listeners { get; set; }

        protected virtual void Awake()
        {
            ItemSlot = new InventoryItem[Width, Height];

            InventoryNetworkList = new NetworkList<InventoryItemData>
                (
                    new List<InventoryItemData>(),
                    NetworkVariableReadPermission.Everyone,
                    NetworkVariableWritePermission.Owner
                );
        }
        protected virtual void Start()
        {
            
        }

        protected virtual void InitializeItems()
        {
            // TODO:

            // foreach (InventoryItemData itemData in InventoryNetworkList)
            // {
            //     InventoryItem item = GameManager.Singleton.InventoryController.CreateItem(itemData);
            //     ItemList.Add(item);
            // }
        }

        public override void OnNetworkSpawn()
        {
            InventoryNetworkList.OnListChanged += OnInventoryNetworkListChanged;
        }
        private void OnInventoryNetworkListChanged(NetworkListEvent<InventoryItemData> changeEvent)
        {
            // Link com mais informações em como usar o NetworkListEvent 
            // https://discussions.unity.com/t/how-to-use-networklist/947471/2

            if(!UpdateLocalItemList()) return;

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Add)
            {
                InventoryItem itemCreated = GameManager.Singleton.InventoryController.CreateItem(changeEvent.Value);
                AddInventoryItem(itemCreated, itemCreated.Data.Position);
                // PlaceItemOnGrid(itemCreated, itemCreated.Data.Position);
                UpdateFromInventory();
            }

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Remove)
            {
                for (int i = 0; i < ItemList.Count; i++)
                {
                    InventoryItem item = ItemList[i];
                    if (item.Data.Id == changeEvent.Value.Id)
                    {
                        RemoveInventoryItem(item);
                        // RemoveItemFromGrid(item);
                        UpdateFromInventory();
                        break;
                    }
                }
            }
        }

        private InventoryItem CreateItem( InventoryItemData data)
        {
            return GameManager.Singleton.InventoryController.CreateItem(data);
        }

        private bool UpdateLocalItemList()
        {
            List<InventoryItemData> tempList = new();
            foreach (InventoryItemData data in InventoryNetworkList)
            {
                tempList.Add(data);
            }

            bool isEqual = true;

            int networkListCount = tempList.Count;
            int inventoryDataListCount = InventoryLocalList.Count;

            if (networkListCount != inventoryDataListCount)
            {
                isEqual = false;
            }
            else
            {
                for (int i = 0; i < networkListCount; i++)
                {
                    if (!tempList[i].Equals(InventoryLocalList[i]))
                    {
                        isEqual = false;
                    }
                }
            }

            // If there was no change in the data, do nothing
            if (isEqual)
            {
                return false;
            }

            // Update InventoryLocalList
            InventoryLocalList.Clear();
            InventoryLocalList = tempList;

            return true;
        }
        public bool AddItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (!AddInventoryItem(inventoryItem, position)) return false;

            // InventoryNetworkList has to be changed after InventoryLocalList
            InventoryLocalList.Add(inventoryItem.GetData());
            InventoryNetworkList.Add(inventoryItem.GetData()); // Checar erro

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

            if(OnRemoveItem != null)
                OnRemoveItem.Raise(this, inventoryItem);

            return true;
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

