using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(InventoryGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private InventoryController inventoryController;
        private InventoryGrid inventoryGrid;

        void Awake()
        {
            inventoryGrid = GetComponent<InventoryGrid>();
        }
        void Start()
        {
            inventoryController = GameManager.Singleton.InventoryController;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.InventoryController.SelectedInventoryGrid = inventoryGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryGrid.ExitGrid();
            GameManager.Singleton.InventoryController.SelectedInventoryGrid = null;
        }
    }
}
