using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "EquipmentSlotType", menuName = "Scriptable Objects/EquipmentSlotType")]
    public class EquipmentType : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
    }
}

