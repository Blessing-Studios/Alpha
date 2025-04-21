using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Backpack", menuName = "Scriptable Objects/Items/Gears/Backpack")]
    public class Backpack : Back
    {
        [Header("Backpack Info")]
        public int GridSizeWidth;
        public int GridSizeHeight;

        public override void Initialize(InventoryItem inventoryItem)
        {
            base.Initialize(inventoryItem);

            if (inventoryItem.Inventory != null) return;

            var container = Instantiate(GameManager.Singleton.ContainerPrefab);

            // container.gameObject.name = inventoryItem.Item.name + "-Container";

            // inventoryItem.Inventory = container.GetComponent<Inventory>();

            // Para testar
            // inventoryItem.Inventory.Owner = inventoryItem.gameObject;

            // inventoryItem.Inventory.Width = GridSizeWidth;
            // inventoryItem.Inventory.Height = GridSizeHeight;

            // container.gameObject.SetActive(true);

            container.Spawn();

            Inventory inventory = container.GetComponent<Inventory>();
            inventoryItem.Inventory = inventory;

            inventory.SetNetworkVariables(GridSizeWidth, GridSizeHeight, inventoryItem);
        }
    }
}

