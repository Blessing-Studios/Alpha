using System.Runtime.InteropServices;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory.Containers
{
    public class ContainerItem : InventoryItem
    {
        [field:SerializeField] public Inventory Inventory { get; protected set; }
    }
}

