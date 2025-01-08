using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Player
{
    // Colocar essa lógica em outro luagr
    public class PlayerGear : CharacterGear
    {
        protected override void Start()
        {
            if (GameManager.Singleton.InventoryController == null)
            {
                Debug.LogError(gameObject.name + " InventoryController is missing");
            }

            if (HasAuthority)
            {
                if (Inventory != null)
                    SetInventory();

                // Temporário
                foreach (BaseGrid grid in GameManager.Singleton.InventoryController.Grids)
                {
                    grid.Owner = this.gameObject;
                    grid.InitializeGrid();
                }
            }

            base.Start();
        }

        public void SetInventory()
        {
            Inventory.InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
            GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = Inventory;
            Inventory.InventoryGrid.Owner = this.gameObject;
            GameManager.Singleton.InventoryController.PlayerInventoryGrid.InitializeGrid();
        }
    }
}

