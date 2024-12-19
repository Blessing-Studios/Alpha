using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(CharacterInventory))]
    public class Trader : MonoBehaviour, IInteractable
    {
        public CharacterInventory TraderInventory { get; private set; }
        public CharacterInventory Customer;
        private Guid itemGuid;
        private Vector2Int itemOriginalPosition;
        
        void Awake()
        {
            TraderInventory = GetComponent<CharacterInventory>();
        }

        public void Interact(Interactor interactor)
        {
            if (interactor.gameObject.TryGetComponent(out CharacterInventory customer))
            {
                if (!TraderInventory.InventoryGrid.IsOpen)
                {
                    // if is closed, open inventoryGrid and get customer
                    this.Customer = customer;
                    TraderInventory.GetOwnership();
                    TraderInventory.InventoryGrid.ToggleInventoryGrid();
                }
                else if (TraderInventory.InventoryGrid.IsOpen)
                {
                    // If is opened, close Inventory and remove customer
                    TraderInventory.InventoryGrid.ToggleInventoryGrid();
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

            if (component.gameObject == Customer.gameObject
                && item.Data.Id == itemGuid)
            {
                PurchaseItem(item);
            }

            if (component.gameObject == gameObject)
            {
                SellItem(item);
            }
        }

        public void OnRemoveItem(Component component, object data)
        {
            if (Customer == null) return;
            
            if (data is not InventoryItem)
            {
                return;
            }

            InventoryItem item = data as InventoryItem;
            
            itemGuid = item.Data.Id;
            itemOriginalPosition = item.GridPosition;

            if (component.gameObject == gameObject)
            {
                ShowItemInfo(item);
                return;
            }
        }

        private void ShowItemInfo(InventoryItem item)
        {
            // Show message with info and price of the item
            Debug.Log(gameObject.name + "Item Id: " + item.Item.Id);
            Debug.Log(gameObject.name + "Item Name: " + item.Item.name);
            Debug.Log(gameObject.name + "Item Value: " + item.Value);

            // Check if player has Gold to buy,
            bool canBuyItem = item.Value < Customer.Gold;
            if (!canBuyItem)
            {
                // Show message that the player is trying to buy item but doesn't have gold
            }
        }

        private void ReserveItemToSell(InventoryItem item)
        {
            itemGuid = item.Data.Id;
            itemOriginalPosition = item.GridPosition;
        }


        private void PurchaseItem(InventoryItem item)
        {
            

            // Get Gold from player
            bool itemBought = Customer.SpendGold(item.Value);

            if (!itemBought)
            {
                // If failed, show message that the item was not bought
                // Block Item from being bought
                CancelPurchase(item);
                return;
            }
            
            // If success, show message that item was bought
            Debug.Log(gameObject.name + "PurchaseItem Customer: " + Customer.gameObject.name);
        }

        private void SellItem(InventoryItem item)
        {
            bool itemSell = Customer.GainGold(item.Value);

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
            Customer.RemoveItem(item);

            // Add Item to original place from trader
            TraderInventory.InventoryGrid.PlaceItem(item, itemOriginalPosition);
            Debug.Log(gameObject.name + "CancelPurchase: " + Customer.gameObject.name);
        }

        private void CancelSell(InventoryItem item)
        {
            // Remove Item from Trader inventory
            TraderInventory.RemoveItem(item);

            // Add Item to original seller
            Customer.InventoryGrid.PlaceItem(item);
        }
    }
}
