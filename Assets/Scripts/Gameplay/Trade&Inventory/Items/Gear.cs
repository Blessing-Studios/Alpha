using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Gear", menuName = "Scriptable Objects/Items/Gear")]
    public class Gear : Item
    {
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference EquipmentType;
        [SerializeField] public EquipmentType GearType { get { return EquipmentType.value as EquipmentType; } }
    }
}
