using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Items/Consumables")]
    public class Consumable : Item
    {
        [Header("Consumable Info")]
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference EquipmentType;
        [SerializeField] public EquipmentType GearType { get { return EquipmentType.value as EquipmentType; } set { EquipmentType.value = value;}}
        public Buff[] Buffs;
    }
}
