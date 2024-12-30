using Blessing.Player;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryController : MonoBehaviour
    {
        public GameObject InventoryCanvas;
        public InventoryGrid PlayerInventoryGrid;
        public EquipmentSlot ChestSlot;
        public IGrid OtherInventoryGrid;
        public IGrid SelectedGrid;
        [SerializeField] private List<Item> itemList = new();
        public GameObject ItemPrefab { get { return GameManager.Singleton.InventoryItemPrefab; } }
        public NetworkObject LooseItemPrefab { get { return GameManager.Singleton.LooseItemPrefab; } }
        private Transform canvasTransform { get { return InventoryCanvas.transform; } }
        [SerializeField] private InventoryItem selectedItem;

        void Awake()
        {
            GameManager.Singleton.InventoryController = this;
        }

        void Start()
        {
            // Criar lógica para desativar o inventory controller dos outros players nesse Client

            if (InventoryCanvas == null)
            {
                Debug.LogError(gameObject.name + " InventoryCanvas is missing");
                // Find Canvas if canvas is null
            }

            if (PlayerInventoryGrid == null)
            {
                Debug.LogError(gameObject.name + " PlayerInventoryGrid is missing");
                // Find Canvas if PlayerInventoryGrid is null
            }
        }

        void Update()
        {
            // Fazer um check para ver se tem algum inventário aberto

            HandleItemDrag();

            HandleHighlight();

            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleGrid(PlayerInventoryGrid);
                ToggleGrid(ChestSlot);
                
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                CreateRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                InsertRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateItem();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                RemoveSelectedItem();
            }

            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseButtonPress();
            }
        }

        private void ToggleGrid(IGrid grid)
        {
            grid.ToggleGrid();
        }

        private void RotateItem()
        {
            if (selectedItem == null) return;

            selectedItem.Rotate();
        }

        private void RemoveSelectedItem()
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            selectedItem.gameObject.SetActive(false); // Temporário
            selectedItem = null;
        }

        private void InsertRandomItem()
        {
            CreateRandomItem();
            if (selectedItem != null) PlaceItem();
        }

        private void HandleItemDrag()
        {
            if (selectedItem == null) return;

            selectedItem.RectTransform.SetParent(canvasTransform);
            selectedItem.RectTransform.position = Input.mousePosition;
        }
        private void HandleHighlight()
        {

            if (SelectedGrid == null) return;

            Vector2Int position = GetTileGridPosition();

            // If the positions are equal, doesn't need to recalculate
            if (SelectedGrid.HighlightPosition == position)
            {
                return;
            }

            if (selectedItem == null)
            {
                InventoryItem item = SelectedGrid.GetItem(position);
                if (item != null)
                {
                    SelectedGrid.SetHighlight(item);
                }
                else
                {
                    SelectedGrid.RemoveHighlight();
                }
            }
            else if (selectedItem != null)
            {
                SelectedGrid.SetHighlight(selectedItem, GetTileGridPosition());
            }
        }
        public InventoryItem CreateItem(Item item)
        {
            InventoryItem inventoryItem = Instantiate(ItemPrefab).GetComponent<InventoryItem>();
            inventoryItem.gameObject.SetActive(false);
            inventoryItem.RectTransform.SetParent(canvasTransform);

            
            inventoryItem.Set(item);
            return inventoryItem;
        }
        public InventoryItem CreateItem(int id)
        {
            InventoryItem inventoryItem = Instantiate(ItemPrefab).GetComponent<InventoryItem>();
            inventoryItem.gameObject.SetActive(false);
            inventoryItem.RectTransform.SetParent(canvasTransform);

            foreach (Item item in itemList)
            {
                if (item.Id == id)
                {
                    inventoryItem.Set(item);
                    return inventoryItem;
                }
            }

            return null;
        }

        public InventoryItem CreateItem(InventoryItemData data)
        {
            InventoryItem inventoryItem = Instantiate(ItemPrefab).GetComponent<InventoryItem>();
            inventoryItem.gameObject.SetActive(false);
            inventoryItem.RectTransform.SetParent(canvasTransform);

            foreach (Item item in itemList)
            {
                if (item.Id == data.ItemId)
                {
                    inventoryItem.Set(item, data);
                    return inventoryItem;
                }
            }

            return null;
        }

        public void CreateRandomItem() // Função temporário para teste
        {
            if (SelectedGrid == null) return;

            if (selectedItem != null) return;

            InventoryItem inventoryItem = Instantiate(ItemPrefab).GetComponent<InventoryItem>();

            inventoryItem.RectTransform.SetParent(canvasTransform);
            inventoryItem.Set(itemList[UnityEngine.Random.Range(0, itemList.Count)]);

            selectedItem = inventoryItem;
        }
        private void LeftMouseButtonPress() // Organizar
        {
            if (SelectedGrid == null && selectedItem != null)
            {
                MoveItemToGround();
                return;
            }

            if (SelectedGrid == null) return;

            Vector2Int tileGridPosition = GetTileGridPosition();

            if (selectedItem == null)
            {
                PickUpItem(tileGridPosition);
            }
            else
            {
                PlaceItem(tileGridPosition);
            }
        }

        public Vector2Int GetTileGridPosition()
        {
            Vector2 position = Input.mousePosition;

            if (selectedItem != null)
            {
                position.x -= (selectedItem.Width - 1) * BaseGrid.TileSizeWidth / 2;
                position.y += (selectedItem.Height - 1) * BaseGrid.TileSizeHeight / 2;
            }

            return SelectedGrid.GetTileGridPosition(position);
        }
        private void PlaceItem()
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            if (SelectedGrid.PlaceItem(selectedItem))
                selectedItem = null;
        }
        private void PlaceItem(Vector2Int position)
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            if (SelectedGrid.PlaceItem(selectedItem, position))
                selectedItem = null;
        }

        private void PickUpItem(Vector2Int position)
        {
            selectedItem = SelectedGrid.PickUpItem(position);
        }

        public void MoveItemToGround()
        {
            InventoryItem inventoryItem = selectedItem;

            GameObject owner = PlayerInventoryGrid.Inventory.gameObject;
            
            var looseItem = Instantiate(LooseItemPrefab, position: owner.transform.position, rotation: owner.transform.rotation);

            looseItem.GetComponent<LooseItem>().InventoryItem = inventoryItem;

            looseItem.Spawn();

            inventoryItem.gameObject.SetActive(false);

            selectedItem = null;
        }
    }
}

