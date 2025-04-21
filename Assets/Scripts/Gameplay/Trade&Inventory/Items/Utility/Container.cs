using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Container", menuName = "Scriptable Objects/Items/Gears/Utility/Container")]
    public class Container : Utility
    {
        [Header("Container")]
        [ScriptableObjectDropdown(typeof(ItemType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        [SerializeField] private ScriptableObjectReference containerType;
        public ItemType ContainerType { get { return containerType.value as ItemType; } set { containerType.value = value;}}
        public int ItemsMaxStack = 10;
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

            inventory.ContainerType = ContainerType;
            inventory.ItemsMaxStack = ItemsMaxStack; 
            inventory.SetNetworkVariables(GridSizeWidth, GridSizeHeight, inventoryItem);
        }
    }
}
