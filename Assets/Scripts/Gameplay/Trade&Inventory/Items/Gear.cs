using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters.Traits;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Gear", menuName = "Scriptable Objects/Items/Gear")]
    public class Gear : Item
    {
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference EquipmentType;
        [SerializeField] public EquipmentType GearType { get { return EquipmentType.value as EquipmentType; } }
        public List<Trait> Traits;
    }
}
