using Blessing.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryController : MonoBehaviour
    {
        public GameObject InventoryCanvas;
        public InventoryGrid PlayerInventoryGrid;
        public InventoryGrid OtherInventoryGrid;
        public InventoryGrid SelectedInventoryGrid;
        [SerializeField] private List<Item> itemList = new();
        [SerializeField] private GameObject itemPrefab;
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
            
            PlayerInventoryGrid.gameObject.SetActive(false);
        }
        
        void Update()
        {
            // Fazer um check para ver se tem algum inventário aberto

            HandleItemDrag();

            HandleHighlight();

            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventoryGrid(PlayerInventoryGrid);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                ToggleInventoryGrid(OtherInventoryGrid);
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

        private void ToggleInventoryGrid(InventoryGrid inventoryGrid)
        {
            inventoryGrid.ToggleInventoryGrid();
        }

        private void RotateItem()
        {
            if (selectedItem == null) return;

            selectedItem.Rotate();
        }

        private void RemoveSelectedItem()
        {
            if (selectedItem == null) return;
            if (SelectedInventoryGrid == null) return;

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
            
            if (SelectedInventoryGrid == null) return;

            Vector2Int position = GetTileGridPosition();

            if (SelectedInventoryGrid.HighlightPosition == position) 
            {
                return;
            }

            if (selectedItem == null)
            {
                InventoryItem item = SelectedInventoryGrid.GetItem(position);
                if (item != null)
                {
                    SelectedInventoryGrid.SetHighlight(item);
                }
                else
                {
                    SelectedInventoryGrid.RemoveHighlight();
                }
            }
            else if (selectedItem != null)
            {
                SelectedInventoryGrid.SetHighlight(selectedItem, GetTileGridPosition());
            }
        }

        public InventoryItem CreateItem(int id)
        {
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            inventoryItem.RectTransform.SetParent(canvasTransform);

            foreach(Item item in itemList)
            {
                if (item.Id == id)
                {
                    inventoryItem.Set(item);
                    return inventoryItem;
                }
            }

            return null;
        }

        public InventoryItem CreateItem (InventoryItemData data)
        {
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            inventoryItem.RectTransform.SetParent(canvasTransform);

            foreach(Item item in itemList)
            {
                if (item.Id == data.ItemId)
                {
                    inventoryItem.Set(item, data);
                    return inventoryItem;
                }
            }

            return null;
        }

        public void CreateRandomItem()
        {
            if (selectedItem != null) return;
            
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();

            inventoryItem.RectTransform.SetParent(canvasTransform);
            inventoryItem.Set(itemList[UnityEngine.Random.Range(0, itemList.Count)]);

            selectedItem = inventoryItem;
        }
        private void LeftMouseButtonPress()
        {
            if (SelectedInventoryGrid == null) return;

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
                position.x -= (selectedItem.Width -1) * InventoryGrid.TileSizeWidth / 2;
                position.y += (selectedItem.Height - 1) * InventoryGrid.TileSizeHeight / 2;
            }

            return SelectedInventoryGrid.GetTileGridPosition(position);
        }
        private void PlaceItem()
        {
            if (selectedItem == null) return;
            if (SelectedInventoryGrid == null) return;

            Vector2Int? position = SelectedInventoryGrid.FindEmptyPosition(selectedItem.Width, selectedItem.Height);

            if (position != null) 
                PlaceItem((Vector2Int) position);
        }
        private void PlaceItem(Vector2Int position)
        {
            if (selectedItem == null) return;
            if (SelectedInventoryGrid == null) return;

            if (SelectedInventoryGrid.PlaceItem(selectedItem, position))
                selectedItem = null;
        }

        private void PickUpItem(Vector2Int position)
        {
            selectedItem = SelectedInventoryGrid.PickUpItem(position);
        }
    }
}

