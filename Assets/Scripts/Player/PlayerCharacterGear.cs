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

            base.Start();
            SetGrids();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            base.OnOwnershipChanged(previous, current);
            SetGrids();
        }

        private void SetGrids()
        {
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
        }

        private void SetInventory()
        {
            Inventory.InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
            GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = Inventory;
            Inventory.InventoryGrid.Owner = this.gameObject;
            GameManager.Singleton.InventoryController.PlayerInventoryGrid.InitializeGrid();
        }

        public override void AddBackpack(InventoryItem inventoryItem)
        {
            base.AddBackpack(inventoryItem);
            
            // If this is the Local Player, change PlayerInventoryGrid
            if (HasAuthority)
            { 
                GameManager.Singleton.InventoryController.PlayerCharacter = character as PlayerCharacter;
                GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = Inventory;
                Inventory.InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
            }
        }

        public override void RemoveBackpack()
        {
            // If this is the Local Player, change PlayerInventoryGrid
            if (Inventory != null && HasAuthority)
            {
                GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = null;
                Inventory.InventoryGrid = GameManager.Singleton.InventoryController.OtherInventoryGrid;
            }

            base.RemoveBackpack();
        }
    }
}

