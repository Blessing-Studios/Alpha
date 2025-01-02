using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "EquipmentType", menuName = "Scriptable Objects/EquipmentType")]
    public class EquipmentType : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
    }
}

