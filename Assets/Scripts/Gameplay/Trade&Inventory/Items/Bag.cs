using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Bag", menuName = "Scriptable Objects/Items/Gears/Bag")]
    public class Bag : Gear
    {
        [Header("Bag Info")]
        public int GridSizeWidth;
        public int gridSizeHeight;
        public Inventory Inventory;
    }
}
