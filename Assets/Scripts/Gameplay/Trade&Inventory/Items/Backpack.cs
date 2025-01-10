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

        public override void Initialize(InventoryItem inventoryItem)
        {
            base.Initialize(inventoryItem);

            var container = Instantiate(GameManager.Singleton.ContainerPrefab);

            // container.gameObject.name = inventoryItem.Item.name + "-Container";

            // inventoryItem.Inventory = container.GetComponent<Inventory>();

            // Para testar
            // inventoryItem.Inventory.Owner = inventoryItem.gameObject;

            // inventoryItem.Inventory.Width = GridSizeWidth;
            // inventoryItem.Inventory.Height = GridSizeHeight;

            // container.gameObject.SetActive(true);

            container.Spawn();

            container.GetComponent<Inventory>().SetNetworkVariables(GridSizeWidth, GridSizeHeight, inventoryItem);
        }
    }
}

