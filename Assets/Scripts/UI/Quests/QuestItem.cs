using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.UI.Quests
{
    public class QuestItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public InventoryItem InventoryItem;
        public TextMeshProUGUI QuantityText;

        public void Initialize(Item item, int quantity = 1)
        {
            InventoryItem.Set(item);

            if (quantity <= 1)
            {
                QuantityText.gameObject.SetActive(false);
            }
            else
            {
                QuantityText.text = $"x {quantity}";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.ItemInfoBox.OpenInfoBox(InventoryItem, transform.position + new Vector3(10, 0, 0), true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Singleton.ItemInfoBox.CloseInfoBox();
        }
    }
}
