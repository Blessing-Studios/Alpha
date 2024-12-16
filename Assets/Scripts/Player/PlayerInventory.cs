using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerInventory : CharacterInventory
    {
        protected override void Start()
        {
            
            InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
            InventoryGrid.Inventory = this;

            if (GameManager.Singleton.InventoryController == null)
            {
                Debug.LogError(gameObject.name + " InventoryController is missing");
            }

            GameManager.Singleton.InventoryController.PlayerInventoryGrid.InitializeGrid();

            base.Start();
        }
    }
}

