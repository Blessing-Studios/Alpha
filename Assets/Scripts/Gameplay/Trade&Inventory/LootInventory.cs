using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class LootInventory : Inventory
    {
        protected override void Start()
        {
            
            InventoryGrid = GameManager.Singleton.InventoryController.OtherInventoryGrid;
            InventoryGrid.Inventory = this;

            if (GameManager.Singleton.InventoryController == null)
            {
                Debug.LogError(gameObject.name + " InventoryController is missing");
            }

            GameManager.Singleton.InventoryController.OtherInventoryGrid.InitializeGrid();

            base.Start();
        }
    }
}

