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
        public int Stack;
        public TradeOperation Operation; 
        public Vector2Int OriginalPosition;
    }
    public class Trader : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [field: SerializeField] public Inventory Inventory { get; protected set; }
        public Character Customer;
        [Tooltip("Coins the trader can use")] public Coin[] Coins;
        [SerializeField] protected List<Coin> orderedCoinsList = new();
        public List<Trade> CurrentTrades;
        public int CurrentTradesTotalValue;
        private FixedString64Bytes reservedItemGuid;
        private Vector2Int reservedItemPosition;
        private bool isItemMoving = false;
        protected Animator animator;
        protected int isOpenHash;
        protected Dictionary<Coin, int> valueByCoins = new();
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
            if (Coins.Length == 0)
                Debug.LogError(gameObject.name + ": Trader needs to have coins to trade");

            // Order coins by most valuable
            orderedCoinsList = Coins.OrderByDescending(x => x.Value).ToList();

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

        protected virtual void OpenTrader(Character customer)
        {
            Customer = customer;

            UIController.Singleton.OpenTraderUI(this);
        }

        protected virtual void CloseTrader()
        {
            Customer = null;

            UIController.Singleton.CloseTraderUI();
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
            }

            // If foundTrade doesn't have item, create new Trade for the item
            CurrentTrades.Add(new Trade() {
                InventoryItem = item,
                Value = item.Value * item.Stack,
                Stack = item.Stack,
                Operation = operation,
                OriginalPosition = selectedItemPosition
            });
        }

        public void RemoveItemFromShoppingList(InventoryItem inventoryItem)
        {

        }

        public void OnRemoveItem(Component component, object data)
        {
            Debug.Log("OnRemoveItem");

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

        public bool TradeCoins()
        {
            // Calculate the diference that still needs to be paid
            int subTotal = 0;
            foreach (Trade trade in CurrentTrades)
            {
                if (trade.Operation == TradeOperation.Buy)
                    subTotal -= trade.Value;

                
                if (trade.Operation == TradeOperation.Sell)
                    subTotal += trade.Value;
            }

            // If customer is buying more than selling
            if (subTotal < 0)
            {
                // Look for coins in the Customer to pay
                List<InventoryItem> customerCoins = Customer.Gear.CoinsToPay(-subTotal, Coins);

                int customerValuePayed = 0;
                foreach (InventoryItem coinItem in customerCoins)
                {
                    customerValuePayed += coinItem.Item.Value * coinItem.Data.Stack;
                }

                // If customer doesn't have enough coins, send menssage
                if (customerValuePayed + subTotal < 0)
                {
                    return false;
                }

                for (int i = customerCoins.Count - 1; i >= 0; i--)
                {
                    // Debug.Log("CoiItem Info: Name - " + customerCoins[i].Item.Label + " Id - " + customerCoins[i].Data.Id);
                    Customer.Gear.RemoveItem(customerCoins[i]);
                    customerCoins[i].Release();
                }

                // Add coins to subtotal, so it can calculate in the trade
                    subTotal += customerValuePayed;
            }

            // If customer is selling more than buying, 
            if (subTotal > 0)
            {
                // add coins from trader in the Trade

                int subTotalRest = subTotal;
                foreach (Coin coin in orderedCoinsList)
                {

                    int coinsQty = subTotalRest / coin.Value;

                    if (coinsQty == 0) continue;

                    // Spawn coinsQty of the coin in the loop
                    InventoryItem coinItem = UIController.Singleton.GetInventoryItem();
                    coinItem.Set(coin, coinsQty);
                    // AddItemToShoppingList(coinItem, TradeOperation.Buy);

                    Customer.Gear.AddItem(coinItem);

                    subTotalRest %= coin.Value;
                }
            }

            return true;
        }

        public bool ConfirmTrades()
        {
            if (!TradeCoins())
            {
                // If faild to pay, send mensagem that coins are missing
                Debug.Log("ConfirmTrades failed - Can't Pay");
                return false;
            }

            for (int i = CurrentTrades.Count - 1; i >= 0; i--)
            {
                // Remove symbol that item is in shopping list
                CurrentTrades.RemoveAt(i);
            }

            Debug.Log("ConfirmTrades Success");
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
