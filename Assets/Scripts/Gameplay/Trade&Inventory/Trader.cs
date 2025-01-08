using System;
using System.Collections.Generic;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(Inventory))]
    public class Trader : MonoBehaviour, IInteractable
    {
        public Inventory TraderInventory { get; private set; }
        public Character Customer;
        private Guid reservedItemGuid;
        private Vector2Int reservedItemPosition;

        // Para debugar
        [SerializeField] private InventoryItem reservedItem;
        
        void Awake()
        {
            TraderInventory = GetComponent<Inventory>();
        }

        public void Interact(Interactor interactor)
        {
            Debug.Log(gameObject.name + "Interact");
            if (interactor.gameObject.TryGetComponent(out Character customer))
            {
                if (!TraderInventory.InventoryGrid.IsOpen)
                {
                    // if is closed, open inventoryGrid and get customer
                    this.Customer = customer;
                    TraderInventory.GetOwnership();
                    TraderInventory.InventoryGrid.ToggleGrid();
                }
                else if (TraderInventory.InventoryGrid.IsOpen)
                {
                    // If is opened, close Inventory and remove customer
                    TraderInventory.InventoryGrid.ToggleGrid();
                    Customer = null;
                }
            }
        }

        public void OnAddItem(Component component, object data)
        {
            if (Customer == null) return;

            if (data is not InventoryItem)
            {
                return;
            }

            InventoryItem item = data as InventoryItem;
            
            // Temporariamente comentado
            // if (component.gameObject == Customer.gameObject
            //     && item.Data.Id == reservedItemGuid)

            if (item.Data.Id == reservedItemGuid)
            {
                PurchaseItem(item);
            }

            if (component.gameObject == gameObject
                && item.Data.Id != reservedItemGuid)
            {
                SellItem(item);
            }
        }

        public void OnRemoveItem(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (Customer == null) return;

            if (data is not InventoryItem) return;

            InventoryItem item = data as InventoryItem;
            
            ReserveItem(item);

            ShowItemInfo(item);
        }

        private void ShowItemInfo(InventoryItem item)
        {
            // Show message with info and price of the item
            Debug.Log(gameObject.name + "Item Id: " + item.Item.Id);
            Debug.Log(gameObject.name + "Item Name: " + item.Item.name);
            Debug.Log(gameObject.name + "Item Value: " + item.Value);

            // Check if player has Gold to buy,
            bool canBuyItem = item.Value < Customer.Gear.Gold;
            if (!canBuyItem)
            {
                // Show message that the player is trying to buy item but doesn't have gold
            }
        }

        private void ReserveItem(InventoryItem item)
        {
            reservedItemGuid = item.Data.Id;
            reservedItemPosition = item.GridPosition;
            reservedItem = item;
        }

        private void CleanReserveItem()
        {
            reservedItemGuid = Guid.Empty;
            reservedItemPosition = Vector2Int.zero;
            reservedItem = null;
        }


        private void PurchaseItem(InventoryItem item)
        {
            

            // Get Gold from player
            bool itemBought = Customer.Gear.SpendGold(item.Value);

            if (!itemBought)
            {
                // If failed, show message that the item was not bought
                // Block Item from being bought
                CancelPurchase(item);
                return;
            }
            
            // If success, show message that item was bought
            Debug.Log(gameObject.name + "PurchaseItem Customer: " + Customer.gameObject.name);

            CleanReserveItem();
        }

        private void SellItem(InventoryItem item)
        {
            bool itemSell = Customer.Gear.GainGold(item.Value);

            if (!itemSell)
            {
                CancelSell(item);
                return;
            }

            // If success, show message that item was bought
            Debug.Log(gameObject.name + "SellItem Seller: " + Customer.gameObject.name);
        }

        private void CancelPurchase(InventoryItem item)
        {
            // Remove Item from buyer inventory
            Customer.Gear.Inventory.RemoveItem(item);

            // Add Item to original place from trader
            TraderInventory.InventoryGrid.PlaceItem(item, reservedItemPosition);
            Debug.Log(gameObject.name + "CancelPurchase: " + Customer.gameObject.name);
        }

        private void CancelSell(InventoryItem item)
        {
            // Remove Item from Trader inventory
            TraderInventory.RemoveItem(item);

            // Add Item to original seller
            Customer.Gear.Inventory.InventoryGrid.PlaceItem(item);
        }
    }
}
