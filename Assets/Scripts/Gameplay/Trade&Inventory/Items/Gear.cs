using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Gear", menuName = "Scriptable Objects/Items/Gears")]
    public class Gear : Item
    {
        [Header("Gear Info")]
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference EquipmentType;
        [SerializeField] public EquipmentType GearType { get { return EquipmentType.value as EquipmentType; } set { EquipmentType.value = value;}}
        public List<Trait> Traits;
    }
}
