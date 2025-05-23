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
            if (UIController.Singleton == null)
            {
                Debug.LogError(gameObject.name + " UIController is missing");
            }

            base.Start();
            // SetInventoryGrids();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            base.OnOwnershipChanged(previous, current);
            // SetInventoryGrids();
        }

        private void SetInventoryGrids()
        {
            // if (HasAuthority)
            // { 
            //     UIController.Singleton.SetPlayerCharacter(character);   
            // }
        }

        // public override void AddBackpack(InventoryItem inventoryItem)
        // {
        //     base.AddBackpack(inventoryItem);
            
        //     // If this is the Local Player, change PlayerInventoryGrid
        //     if (HasAuthority)
        //     { 
        //         UIController.Singleton.PlayerCharacterInventoryUI.SetBackpackInventoryGrid(Inventory);
        //     }
        // }
        // public override void AddUtility(InventoryItem inventoryItem, int duplicatedIndex = 0)
        // {
        //     base.AddUtility(inventoryItem, duplicatedIndex);

        //     if (HasAuthority)
        //     {
        //         UIController.Singleton.PlayerCharacterInventoryUI.SetUtilityInventoryGrids(UtilityInventories);
        //     }
        // }

        // public override void RemoveBackpack()
        // {
        //     base.RemoveBackpack();

        //     // If this is the Local Player, change PlayerInventoryGrid
        //     if (HasAuthority)
        //     {
        //         UIController.Singleton.PlayerCharacterInventoryUI.SetBackpackInventoryGrid(Inventory);
        //     }

            
        // }

        // public override void RemoveUtility(int duplicatedIndex)
        // {
        //     base.RemoveUtility(duplicatedIndex);

        //     if (HasAuthority)
        //     {
        //         UIController.Singleton.PlayerCharacterInventoryUI.SetUtilityInventoryGrids(UtilityInventories);
        //     }
        // }
    }
}

