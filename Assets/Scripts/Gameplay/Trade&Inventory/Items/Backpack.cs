using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Backpack", menuName = "Scriptable Objects/Items/Gears/Backpack")]
    public class Backpack : Gear
    {
        [Header("Backpack Info")]
        public int GridSizeWidth;
        public int gridSizeHeight;
    }
}

