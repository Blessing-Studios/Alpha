using System;
using Blessing.Core.GameEventSystem;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Services.Vivox;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{ 
    [Serializable]
    public class CharacterEquipment
    {
        public string Name;
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference SlotType;
        [SerializeField] public EquipmentType GearSlotType { get { return SlotType.value as EquipmentType; } }
        public InventoryItem InventoryItem;
        public bool SetEquipment(InventoryItem inventoryItem)
        {
            if (InventoryItem != null && inventoryItem.Data.Equals(InventoryItem.Data)) return false;

            Gear gear = inventoryItem.Item as Gear;

            if (gear == null) return false;

            if (gear.GearType != GearSlotType) return false;

            InventoryItem = inventoryItem;
            
            return true;
        }

        public void Unequip()
        {
           InventoryItem = null;
        }
    }
}