using System;
using System.Collections.Generic;
using System.Linq;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Unity.Collections;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public enum TradeOperation { Buy, Sell }
    [Serializable] public struct Trade
    {
        public InventoryItem InventoryItem;
        public int Value;
        public TradeOperation Operation; 
        public Vector2Int OriginalPosition;
    }
    public class Trader : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [field: SerializeField] public Inventory Inventory { get; protected set; }
        public Character Customer;
        public List<Trade> CurrentTrades;
        public int CurrentTradesTotalValue;
        private FixedString64Bytes reservedItemGuid;
        private Vector2Int reservedItemPosition;
        private bool isItemMoving = false;
        protected Animator animator;
        protected int isOpenHash;
        public TraderInventoryUI TraderInventoryUI { get { return UIController.Singleton.TraderInventoryUI; } }
        public bool CanInteract { get { return true; } }

        // Para debugar
        [SerializeField] private InventoryItem reservedItem;
        private Vector2Int selectedItemPosition;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();

            isOpenHash = Animator.StringToHash("IsOpen");
        }

        protected virtual void Start()
        {
            // 
        }

        protected virtual void Update()
        {
            HandleStopInteraction();
        }

        private void HandleStopInteraction()
        {
            if (Customer == null) return;

            if (!Inventory.InventoryGrid.IsOpen) return;

            float maxDistance = (float)(Customer.Stats.Dexterity + Customer.Stats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, Customer.transform.position);

            if (distance > maxDistance)
            {
                CloseTrader();
            }
        }
        public virtual void Interact(Interactor interactor)
        {
            if (interactor.gameObject.TryGetComponent(out Character customer))
            {
                Inventory.GetOwnership();

                if (!UIController.Singleton.IsGridsOpen)
                {
                    OpenTrader(customer);
                }
                else if (UIController.Singleton.IsGridsOpen)
                {
                    CloseTrader();
                }
            }
        }

        private void OpenTrader(Character customer)
        {
            Customer = customer;

            UIController.Singleton.OpenTraderUI(this);

            if (animator.GetBool(isOpenHash)) return;

            animator.SetTrigger("Open");
            animator.SetBool(isOpenHash, true);
        }

        private void CloseTrader()
        {
            Customer = null;

            UIController.Singleton.CloseTraderUI();

            if (!animator.GetBool(isOpenHash)) return;

            animator.SetTrigger("Close");
            animator.SetBool(isOpenHash, false);
        }

        public void OnAddItem(Component component, object data)
        {
            if (Customer == null) return;

            if (isItemMoving == true) return;

            if (data is not InventoryItem)
            {
                return;
            }

            InventoryItem item = data as InventoryItem;

            
            if (UIController.Singleton.TraderInventoryUI.TraderInventoryGrid.gameObject == component.gameObject)
            {
                // Foi adicionado no grid do trader
                AddItemToShoppingList(item, TradeOperation.Sell);
            }
            else
            {
                AddItemToShoppingList(item, TradeOperation.Buy);
            }    

            CleanReserveItem();
            TraderInventoryUI.UpdateTraderItems();
        }

        public void AddItemToShoppingList(InventoryItem item, TradeOperation operation)
        {
            // Can't sell Reserved item, This item is coming from trader
            if (item.Data.Id == reservedItemGuid && operation == TradeOperation.Sell)
            {
                ReturnReserveItemToTrader();
                return;
            }

            // Can only buy Reserved Item, This is coming from customer
            if (item.Data.Id != reservedItemGuid && operation == TradeOperation.Buy)
            {
                return;
            }

            // Procura se esse item já está na lista
            Trade foundTrade = CurrentTrades.SingleOrDefault(x => x.InventoryItem == item);
            if (foundTrade.InventoryItem != null)
            {
                if (foundTrade.Operation != operation)
                {
                    CurrentTrades.Remove(foundTrade);
                    return;
                }

                if (foundTrade.Operation != operation)
                {
                    return;
                }
            }

            // If foundTrade doesn't have item, create new Trade for the item
            CurrentTrades.Add(new Trade() {
                InventoryItem = item,
                Value = item.Value,
                Operation = operation,
                OriginalPosition = selectedItemPosition
            });
        }

        public void RemoveItemFromShoppingList(InventoryItem inventoryItem)
        {

        }

        public void OnRemoveItem(Component component, object data)
        {
            if (Customer == null) return;
            if (isItemMoving == true) return;
            if (data is not InventoryItem) return;

            InventoryItem item = data as InventoryItem;

            selectedItemPosition = item.GridPosition;

            if (UIController.Singleton.TraderInventoryUI.TraderInventoryGrid != component) return;

            

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
            reservedItemGuid = Guid.Empty.ToString();
            reservedItemPosition = Vector2Int.zero;
            reservedItem = null;
        }

        private void ReturnReserveItemToTrader()
        {
            if (reservedItem != null)
            {
                isItemMoving = true;
                Inventory.RemoveItem(reservedItem);
                Inventory.InventoryGrid.PlaceItem(reservedItem, reservedItemPosition);
                CleanReserveItem();
                isItemMoving = false;
            }
        }

        public bool ConfirmTrades()
        {
            // Pegar Customer Gold
            int customerGold = Customer.Gear.Gold;

            // Validar compras
            // Chegar se Customer pode pagar por tudo
            int totalToPay = 0;

            foreach(Trade trade in CurrentTrades)
            {
                totalToPay = trade.Operation switch
                {
                    TradeOperation.Sell => totalToPay -= trade.Value,
                    _ => totalToPay += trade.Value
                };
            }

            if (totalToPay > customerGold) return false;


            for (int i = CurrentTrades.Count - 1; i >= 0; i--)
            {
                bool validate = CurrentTrades[i].Operation switch
                {
                    TradeOperation.Sell => Customer.Gear.GainGold(CurrentTrades[i].Value),
                    _ => Customer.Gear.SpendGold(CurrentTrades[i].Value)
                };

                if (validate)
                {
                    // Remove symbol that item is in shopping list
                    CurrentTrades.RemoveAt(i);
                }
                else
                {
                    Debug.LogError(gameObject.name + ": Error Shopping product name: " + CurrentTrades[i].InventoryItem.name + " id: " + CurrentTrades[i].InventoryItem.Item.Id);
                    return false;
                }
            }

            return true;
        }

        public bool CancelTrades()
        {
            Debug.Log("CancelTrades");
            bool validate = true;

            for (int i = CurrentTrades.Count - 1; i >= 0; i--)
            {
                validate = CancelTrade(CurrentTrades[i]);
                if (validate)
                {
                    if (ShowDebug) Debug.Log(gameObject.name + ": Trade canceled");
                }
                else
                {
                    Debug.LogError(gameObject.name + ": Error Canceling trade - product name: " + CurrentTrades[i].InventoryItem.name + " id: " + CurrentTrades[i].InventoryItem.Item.Id);
                    return false;
                }
            }

            return validate;
        }

        public bool CancelTrade(Trade trade)
        {
            Debug.Log("CancelTrade");

            isItemMoving = true;
            if (trade.Operation == TradeOperation.Buy)
            {
                Customer.Gear.Inventory.RemoveItem(trade.InventoryItem);
                Inventory.InventoryGrid.PlaceItem(trade.InventoryItem, trade.OriginalPosition);
            }

            if (trade.Operation == TradeOperation.Sell)
            {
                Inventory.RemoveItem(trade.InventoryItem);
                Customer.Gear.Inventory.InventoryGrid.PlaceItem(trade.InventoryItem, trade.OriginalPosition);
            }
            isItemMoving = false;

            bool validate = CurrentTrades.Remove(trade);

            return validate;
        }
    }
}
