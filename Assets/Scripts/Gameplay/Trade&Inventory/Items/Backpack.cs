using Blessing.Gameplay.TradeAndInventory.Containers;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Backpack", menuName = "Scriptable Objects/Items/Gears/Backpack")]
    public class Backpack : Gear
    {
        [Header("Backpack Info")]
        public int GridSizeWidth;
        public int GridSizeHeight;

        public override void Initialize(Component component)
        {
            base.Initialize(component);

            InventoryItem inventoryItem = component as InventoryItem;

            var container = Instantiate(GameManager.Singleton.ContainerPrefab);

            inventoryItem.Inventory = container.GetComponent<Inventory>();
            inventoryItem.Inventory.Width = GridSizeWidth;
            inventoryItem.Inventory.Height = GridSizeHeight;

            container.Spawn();

            container.gameObject.SetActive(true);
        }
    }
}

