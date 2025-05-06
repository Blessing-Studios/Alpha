using Blessing.Core.ObjectPooling;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class TraderItem : PooledObject
    {
        public Trade Trade;
        public Image Icon;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI PriceText;
        public Button RemoveButton;

        public void SetTrade(Trade trade)
        {
            Trade = trade;

            Icon.sprite = trade.InventoryItem.Item.Sprite;
            NameText.text = $"{trade.Operation} - {trade.InventoryItem.Item.Label}";
            PriceText.text = $"{trade.Value} Gold";
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }
        public override void ReleaseToPool()
        {
            RemoveButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
