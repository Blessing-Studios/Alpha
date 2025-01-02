using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Player
{
    // Colocar essa lógica em outro luagr
    public class PlayerInventory : CharacterInventory
    {
        protected override void Start()
        {
            if (GameManager.Singleton.InventoryController == null)
            {
                Debug.LogError(gameObject.name + " InventoryController is missing");
            }

            if (HasAuthority)
            {
                InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
                InventoryGrid.Inventory = this;
                InventoryGrid.Owner = this.gameObject;
                GameManager.Singleton.InventoryController.PlayerInventoryGrid.InitializeGrid();


                // Temporário
                foreach (BaseGrid grid in GameManager.Singleton.InventoryController.Grids)
                {
                    grid.Owner = this.gameObject;
                    grid.InitializeGrid();
                }
            }
            
            base.Start();
        }
    }
}

