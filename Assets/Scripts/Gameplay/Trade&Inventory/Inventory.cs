using Blessing.GameEventSystem;
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
        public List<GameEventListener> Listeners { get; set; }

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
            if (InventoryGrid == null)
            {
                {
                    Debug.LogError(gameObject.name + ": InventoryGrid is missing");
                }
            }
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
            

            List<InventoryItemData> tempList = new();
            foreach(InventoryItemData data in InventoryNetworkList)
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
                for(int i = 0; i < networkListCount; i++)
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
                return;
            }

            // Update InventoryLocalList
            InventoryLocalList.Clear();
            InventoryLocalList = tempList;
            

            // ############

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Add)
            {
                InventoryItem itemCreated = GameManager.Singleton.InventoryController.CreateItem(changeEvent.Value);
                AddInventoryItem(itemCreated, itemCreated.Data.Position);
                InventoryGrid.PlaceItemOnGrid(itemCreated, itemCreated.Data.Position);
            }

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Remove)
            {

                // Search the removed Item so it can be removed from local
                foreach (InventoryItem item in ItemList)
                {
                    if (item.Data.Id == changeEvent.Value.Id)
                    {
                        RemoveInventoryItem(item);
                        InventoryGrid.RemoveItemFromGrid(item);
                    }
                }
            }

            //InventoryGrid.UpdateFromInventory();
        }

        public bool AddItem(InventoryItem inventoryItem, Vector2Int position)
        {
            
            if (!AddInventoryItem(inventoryItem, position)) return false;

            // InventoryNetworkList has to be changed after InventoryLocalList
            InventoryLocalList.Add(inventoryItem.GetData());
            InventoryNetworkList.Add(inventoryItem.GetData()); // Checar erro

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

            int id = inventoryItem.Item.Id;

            ItemList.Add(inventoryItem);

            OnAddItem.Raise(this, id);

            return true;
        }

        public bool RemoveItem(InventoryItem inventoryItem)
        {
            // IF item is not in the inventory, return false
            if (!RemoveInventoryItem(inventoryItem)) return false;
            
            // InventoryNetworkList has to be changed after InventoryLocalList
            InventoryLocalList.Remove(inventoryItem.GetData());
            InventoryNetworkList.Remove(inventoryItem.GetData());

            return true;
        }

        private bool RemoveInventoryItem(InventoryItem inventoryItem)
        {
            if (inventoryItem == null) return false;

            // IF item is not in the inventory, return false
            if (!ItemList.Contains(inventoryItem)) return false;

            int id = inventoryItem.Item.Id;

            ItemList.Remove(inventoryItem);

            OnRemoveItem.Raise(this, id);

            return true;
        }
    }
}

