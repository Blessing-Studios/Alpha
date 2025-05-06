using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class TraderInventoryUI : MonoBehaviour
    {
        public Trader Trader;
        public CharacterInventoryUI CustomerInventoryUI;
        public InventoryGrid TraderInventoryGrid;
        public TraderItem TraderItemPrefab { get { return GameManager.Singleton.TraderItemPrefab; } }
        public List<TraderItem> TraderItems;
        public List<TraderItem> BuyingItems;
        public List<TraderItem> SellingItems;
        public GameObject ItemsContainer;
        public Button ConfirmButton;
        public Button CancelButton;
        [SerializeField] private TextMeshProUGUI buySubtotalText;
        [SerializeField] private TextMeshProUGUI sellSubtotalText;
        [SerializeField] private TextMeshProUGUI totalText;
        public Canvas Canvas { get; private set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void Awake()
        {
            ConfirmButton.onClick.AddListener(() => {
                Trader.ConfirmTrades();
                UpdateTraderItems();
            });

            CancelButton.onClick.AddListener(() => {
                Trader.CancelTrades();
                UpdateTraderItems();
            });
        }
        void Start()
        {
            if (Canvas == null)
                Canvas = GetComponent<Canvas>();
        }
        public void OpenInventoryUI(Trader trader)
        {
            SetTrader(trader);
            gameObject.SetActive(true);
            SyncGrids();
            TraderInventoryGrid.OpenGrid();
            CustomerInventoryUI.OpenInventoryUI();

        }
        public void SetTrader(Trader trader)
        {
            Trader = trader;
            CustomerInventoryUI.SetCharacter(trader.Customer);
            SyncGrids();
        }

        public void CloseInventoryUI()
        {
            if (Trader != null)
                Trader.CancelTrades();

            gameObject.SetActive(false);
        }

        public void SyncGrids()
        {
            CustomerInventoryUI.SyncGrids();

            if (!gameObject.activeSelf) return;

            if (Trader == null) return;

            SetTraderInventoryGrid(Trader);

            TraderInventoryGrid.InitializeGrid();

            UpdateTraderItems();
        }

        public void UpdateTraderItems()
        {
            if (Trader == null) return;

            foreach(TraderItem traderItem in TraderItems)
            {
                traderItem.Release();
            }

            TraderItems.Clear();
            int subTotalBuy = 0;
            int subTotalSell = 0;
            foreach (Trade trade in Trader.CurrentTrades)
            {
                TraderItem newTradeItem = PoolManager.Singleton.Get(TraderItemPrefab) as TraderItem;

                // Testar código 
                newTradeItem.RemoveButton.onClick.AddListener(() => {
                    Trader.CancelTrade(trade);
                    UpdateTraderItems();
                });

                newTradeItem.SetTrade(trade);
                TraderItems.Add(newTradeItem);

                newTradeItem.transform.SetParent(ItemsContainer.transform, false);
                
                if (trade.Operation == TradeOperation.Buy)
                {
                    subTotalBuy += trade.Value;
                    BuyingItems.Add(newTradeItem);
                }

                
                if (trade.Operation == TradeOperation.Sell)
                {
                    subTotalSell += trade.Value;
                    SellingItems.Add(newTradeItem);
                }
            }

            buySubtotalText.text = $"{subTotalBuy}";
            sellSubtotalText.text = $"{subTotalSell}";
            totalText.text = $"{subTotalSell - subTotalBuy}";
        }
        public void SetTraderInventoryGrid(Trader trader)
        {
            if (trader.Inventory == null)
            {
                TraderInventoryGrid.Inventory = null;
                return;
            }

            trader.Inventory.InventoryGrid = TraderInventoryGrid;
            TraderInventoryGrid.Inventory = trader.Inventory;
        }

        public void OnAddItem(Component component, object data)
        {
            if (Trader == null) return;

            Trader.OnAddItem(component, data);
        }

        public void OnRemoveItem(Component component, object data)
        {
            if (Trader == null) return;

            Trader.OnRemoveItem(component, data);
        }
    }
}
