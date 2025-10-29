using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class DiscardItemZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            UIController.Singleton.DiscardItemZone = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIController.Singleton.DiscardItemZone = null;
        }

        public void OnDisable()
        {
            UIController.Singleton.DiscardItemZone = null;
        }
    }
}
