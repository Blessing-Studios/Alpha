using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(IGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private InventoryController inventoryController;
        private IGrid grid;

        void Awake()
        {
            grid = GetComponent<IGrid>();
        }
        void Start()
        {
            inventoryController = GameManager.Singleton.InventoryController;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.InventoryController.SelectedGrid = grid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            grid.ExitGrid();
            GameManager.Singleton.InventoryController.SelectedGrid = null;
        }
    }
}
