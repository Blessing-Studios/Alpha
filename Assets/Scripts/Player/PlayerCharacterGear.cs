using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;
using Blessing.Characters;

namespace Blessing.Player
{
    // Colocar essa lógica em outro luagr
    public class PlayerCharacterGear : CharacterGear
    {
        protected override void Start()
        {
            if (GameManager.Singleton.InventoryController == null)
            {
                Debug.LogError(gameObject.name + " InventoryController is missing");
            }

            if (HasAuthority)
            { 
                GameManager.Singleton.InventoryController.PlayerStatsInfo.CharacterStats = character.CharacterStats;
                GameManager.Singleton.InventoryController.PlayerStatsInfo.Initialize();

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

