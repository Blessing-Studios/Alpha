using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "ItemType", menuName = "Scriptable Objects/ItemType")]
    public class ItemType : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string Label { get; private set; }
    }
}

