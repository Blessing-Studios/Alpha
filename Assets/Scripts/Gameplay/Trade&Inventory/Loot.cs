using Blessing.Gameplay.Interation;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(Inventory))]
    public class Loot : MonoBehaviour, IInteractable
    {
        public int GridSizeWidth;
        public int GridSizeHeight;
        public Inventory Inventory { get; private set; }
        void Awake()
        {
            Inventory = GetComponent<Inventory>();
        }

        void Start()
        {
            Inventory.SetNetworkVariables(GridSizeWidth, GridSizeHeight);
            Inventory.Initialize();
        }

        public void Interact(Interactor interactor)
        {
            if (!Inventory.InventoryGrid.IsOpen)
            {
                Inventory.GetOwnership();
                Inventory.InventoryGrid.ToggleGrid();
            }
            else if (Inventory.InventoryGrid.IsOpen)
            {
                Inventory.InventoryGrid.ToggleGrid();
            }
        }
    }
}

