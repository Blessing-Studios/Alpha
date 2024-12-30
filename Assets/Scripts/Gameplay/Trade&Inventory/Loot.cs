using Blessing.Gameplay.Interation;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(Inventory))]
    public class Loot : MonoBehaviour, IInteractable
    {
        public Inventory Inventory { get; private set; }
        void Awake()
        {
            Inventory = GetComponent<Inventory>();
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

