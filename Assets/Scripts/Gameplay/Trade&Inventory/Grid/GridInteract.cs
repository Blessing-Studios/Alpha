using UnityEngine;
using UnityEngine.EventSystems;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(IGrid))]
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private IGrid grid;

        void Awake()
        {
            grid = GetComponent<IGrid>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UIController.Singleton.SelectedGrid = grid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            grid.ExitGrid();
            UIController.Singleton.SelectedGrid = null;
        }
    }
}
