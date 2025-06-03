using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;
using Blessing.Core.GameEventSystem;

namespace Blessing.Player
{
    // Colocar essa lógica em outro luagr
    public class PlayerCharacterGear : CharacterGear
    {
        public GameEvent OnPlayerAddEquipment;
        public GameEvent OnPlayerRemoveEquipment;
        protected override void Start()
        {
            if (UIController.Singleton == null)
            {
                Debug.LogError(gameObject.name + " UIController is missing");
            }

            base.Start();
            // SetInventoryGrids();
        }

        public override bool AddEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            bool baseValue = base.AddEquipment(equipment, inventoryItem);
            
            // Raise Events
            if (HasAuthority && OnPlayerAddEquipment != null)
                OnPlayerAddEquipment.Raise(this, equipment);

            return baseValue;
        }

        public override bool RemoveEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            bool baseValue = base.RemoveEquipment(equipment, inventoryItem);
            
            // Raise Events
            if (HasAuthority && OnPlayerRemoveEquipment != null)
                OnPlayerRemoveEquipment.Raise(this, equipment);

            return baseValue;
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            base.OnOwnershipChanged(previous, current);
            // SetInventoryGrids();
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

