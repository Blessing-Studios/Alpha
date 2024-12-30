using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Player
{
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
            }
            
            base.Start();
        }
    }
}

