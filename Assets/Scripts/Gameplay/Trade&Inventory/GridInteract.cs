using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(ItemGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private InventoryController inventoryController;
        private ItemGrid itemGrid;

        void Awake()
        {
            itemGrid = GetComponent<ItemGrid>();
        }
        void Start()
        {
            inventoryController = GameManager.Singleton.InventoryController;
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.InventoryController.SelectedItemGrid = itemGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            itemGrid.ExitGrid();
            GameManager.Singleton.InventoryController.SelectedItemGrid = null;
        }
    }
}
