using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;

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
                GameManager.Singleton.InventoryController.PlayerCharacterInventoryUI.SetCharacter(character);   
            }
        }

        // public override void AddBackpack(InventoryItem inventoryItem)
        // {
        //     base.AddBackpack(inventoryItem);
            
        //     // If this is the Local Player, change PlayerInventoryGrid
        //     if (HasAuthority)
        //     { 
        //         GameManager.Singleton.InventoryController.PlayerCharacterInventoryUI.SetBackpackInventoryGrid(Inventory);
        //     }
        // }
        // public override void AddUtility(InventoryItem inventoryItem, int duplicatedIndex = 0)
        // {
        //     base.AddUtility(inventoryItem, duplicatedIndex);

        //     if (HasAuthority)
        //     {
        //         GameManager.Singleton.InventoryController.PlayerCharacterInventoryUI.SetUtilityInventoryGrids(UtilityInventories);
        //     }
        // }

        // public override void RemoveBackpack()
        // {
        //     base.RemoveBackpack();

        //     // If this is the Local Player, change PlayerInventoryGrid
        //     if (HasAuthority)
        //     {
        //         GameManager.Singleton.InventoryController.PlayerCharacterInventoryUI.SetBackpackInventoryGrid(Inventory);
        //     }

            
        // }

        // public override void RemoveUtility(int duplicatedIndex)
        // {
        //     base.RemoveUtility(duplicatedIndex);

        //     if (HasAuthority)
        //     {
        //         GameManager.Singleton.InventoryController.PlayerCharacterInventoryUI.SetUtilityInventoryGrids(UtilityInventories);
        //     }
        // }
    }
}

