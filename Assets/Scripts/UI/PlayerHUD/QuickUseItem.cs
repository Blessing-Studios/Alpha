using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.PlayerHUD
{
    public class QuickUseItem : PooledObject
    {
        public InventoryItem InventoryItem;
        public Image Icon;
        public GameObject BackGround;
        public bool Selected;
        public Button Button;
        [SerializeField] private TextMeshProUGUI stackText;
        public QuickUseItem Initialize(InventoryItem inventoryItem, Transform container, bool selected = false)
        {
            SetItem(inventoryItem);
            transform.SetParent(container, false);
            transform.localPosition = Vector3.zero;
            transform.SetAsLastSibling();
            name = inventoryItem.name;
            Selected = selected;
            Select(selected);

            stackText.text = "";
            if (inventoryItem.Data.Stack > 1)
                stackText.text = inventoryItem.Data.Stack.ToString();

            return this;
        }

        public void UpdateItem()
        {
            stackText.text = "";
            if (InventoryItem.Data.Stack > 1)
                stackText.text = InventoryItem.Data.Stack.ToString();
        }
        public void Select(bool selected = true)
        {
            BackGround.SetActive(selected);
        }
        public void SetItem(InventoryItem inventoryItem)
        {
            InventoryItem = inventoryItem;
            Icon.sprite = inventoryItem.Item.Sprite;
        }
        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }
        public override void ReleaseToPool()
        {
            Button.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
