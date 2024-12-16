using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryGrid : MonoBehaviour
    {
        public Inventory Inventory;
        public List<InventoryItem> GridItems;
        public const float TileSizeWidth = 32;
        public const float TileSizeHeight = 32;
        private RectTransform rectTransform;
        [SerializeField] private int gridSizeWidth = 20;
        [SerializeField] private int gridSizeHeight = 10;
        [SerializeField] GameObject inventoryItemPrefab;
        [field: SerializeField] public RectTransform ItemHighlight { get; protected set; }
        [field: SerializeField] public Vector2Int? HighlightPosition { get; protected set; } 
        private Vector2 positionOnTheGrid = new();
        private Vector2Int tileGridPosition = new();

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void InitializeGrid()
        {   
            bool activated = false;
            if (!gameObject.activeSelf)
            {
                activated = true;
                gameObject.SetActive(true);
            }


            gameObject.SetActive(true);

            if (Inventory == null)
            {
                Debug.LogError(gameObject.name + " Inventory is missing");
            }

            gridSizeWidth = Inventory.Width;
            gridSizeHeight = Inventory.Height;

            rectTransform.sizeDelta = new Vector2(gridSizeWidth * TileSizeWidth, gridSizeHeight * TileSizeHeight);
            ItemHighlight.SetParent(rectTransform);
            RemoveHighlight();

            UpdateFromInventory();

            if (activated)
                gameObject.SetActive(false);
        }

        public void UpdateFromInventory()
        {
            if (Inventory == null) 
            {
                Debug.LogError(gameObject.name + " Inventory is missing");
                return;
            }
            
            CleanInventoryGrid();

            // Refazer Lógica
            // Pega os itens do inventário e coloca no inventoryGrid
            foreach(InventoryItem item in Inventory.ItemList)
            {
                PlaceItemOnGrid(item, item.Data.Position);
            }
        }

        public void CleanInventoryGrid()
        {
            foreach (InventoryItem item in GridItems)
            {
                item.gameObject.SetActive(false);
            }

            GridItems.Clear();
        }

        public void RemoveItemFromGrid(InventoryItem item)
        {
            GridItems.Remove(item);
            item.gameObject.SetActive(false); // Temporário 
        }

        public void ExitGrid()
        {
            RemoveHighlight();
        }

        public Vector2Int GetTileGridPosition(Vector2 mousePosition)
        {
            positionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
            positionOnTheGrid.y = rectTransform.position.y - mousePosition.y;

            tileGridPosition.x = (int)(positionOnTheGrid.x / TileSizeWidth);
            tileGridPosition.y = (int)(positionOnTheGrid.y / TileSizeWidth);

            return tileGridPosition;
        }
        public InventoryItem GetItem(Vector2Int position)
        {
            if (!PositionCheck(position)) return null;

            return Inventory.ItemSlot[position.x, position.y];
        }
        public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (!CheckAvailableSpace(position, inventoryItem.Width, inventoryItem.Height))
            {
                return false;
            }
            // Move the item in the right position on the InventoryGrid
            if (!PlaceItemOnGrid(inventoryItem, position))
            {
                return false;
            }

            // Add Item checking if it is already in Inventory
            if (!Inventory.AddItem(inventoryItem, position))
            {
                return false;
            }

            return true;
        }

        public bool PlaceItemOnGrid(InventoryItem inventoryItem, Vector2Int position)
        {
            inventoryItem.RectTransform.SetParent(this.rectTransform);
            inventoryItem.RectTransform.SetAsLastSibling();

            inventoryItem.Data.Position = position;
            inventoryItem.RectTransform.localPosition = CalculatePosition(position, inventoryItem.Width, inventoryItem.Height);

            GridItems.Add(inventoryItem);

            inventoryItem.gameObject.SetActive(true);

            return true;
        }

        public bool DeleteItem(InventoryItem inventoryItem)
        {
            RemoveItemFromGrid(inventoryItem);

            bool isRemoved = Inventory.RemoveItem(inventoryItem);
            if (isRemoved)
            {
                return true;
            }

            Debug.LogWarning(gameObject.name + " RemoveItem failed");
            return false;
        }

        private bool CheckAvailableSpace(Vector2Int position, int width, int height)
        {
            
            if (!BoundaryCheck(position, width, height)) 
            {
                return false;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (Inventory.ItemSlot[position.x + x, position.y + y] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Vector2 CalculatePosition(Vector2Int position, int width, int height)
        {
            Vector2 localPosition = new()
            {
                x = position.x * TileSizeWidth + TileSizeWidth * width / 2,
                y = -(position.y * TileSizeHeight + TileSizeHeight * height / 2)
            };

            return localPosition;
        }

        internal InventoryItem PickUpItem(Vector2Int position)
        {
            InventoryItem inventoryItem = Inventory.ItemSlot[position.x, position.y];

            if (inventoryItem == null) return null;

            for (int x = 0; x < inventoryItem.Width; x++)
            {
                for (int y = 0; y < inventoryItem.Height; y++)
                {
                    Inventory.ItemSlot[inventoryItem.Data.Position.x + x, inventoryItem.Data.Position.y + y] = null;
                }
            }

            Inventory.RemoveItem(inventoryItem);

            GridItems.Remove(inventoryItem);

            return inventoryItem;
        }
        private bool BoundaryCheck(Vector2Int position, int width, int height)
        {
            if (!PositionCheck(position))
            {
                Debug.Log(gameObject.name + " Position Check 1 failed");
                return false;
            }

            position.x += width - 1;
            position.y += height - 1;

            if (!PositionCheck(position))
            { 
                Debug.Log(gameObject.name + " Position Check 2 failed");
                return false;
            }

            return true;
        }

        private bool PositionCheck(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0) return false;

            if (position.x >= gridSizeWidth || position.y >= gridSizeHeight) return false;

            return true;
        }
        public void SetHighlight(InventoryItem inventoryItem)
        {
            ItemHighlight.gameObject.SetActive(true);

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            // Set Highlight size
            ItemHighlight.sizeDelta = new Vector2() 
            {
                x = width * InventoryGrid.TileSizeWidth,
                y = height * InventoryGrid.TileSizeHeight
            };

            // Set Highlight position
            HighlightPosition = inventoryItem.Data.Position;
            ItemHighlight.localPosition = CalculatePosition(inventoryItem.Data.Position, width, height);
        }

        public void SetHighlight(InventoryItem inventoryItem, Vector2Int position)
        {
            ItemHighlight.gameObject.SetActive(true);

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            // Set Highlight size
            ItemHighlight.sizeDelta = new Vector2() 
            {
                x = width * InventoryGrid.TileSizeWidth,
                y = height * InventoryGrid.TileSizeHeight
            };
            
            // Set Highlight position
            if (!BoundaryCheck(position, width, height))
            {
                RemoveHighlight();
                return;
            }

            HighlightPosition = position;
            ItemHighlight.localPosition = CalculatePosition(position, width, height);
        }

        public void RemoveHighlight()
        {
            HighlightPosition = null;
            ItemHighlight.gameObject.SetActive(false);
        }

        internal Vector2Int? FindEmptyPosition(int width, int height)
        {
            int searchWidth = gridSizeWidth - width + 1;
            int searchHeight = gridSizeHeight - height + 1;

            Vector2Int position;

            for (int y = 0; y < searchHeight; y++)
            {
                for (int x = 0; x < searchWidth; x++)
                {
                    position = new Vector2Int(x, y);
                    if (CheckAvailableSpace(position, width, height)) return position;
                }
            }

            return null;
        }

        public void ToggleInventoryGrid()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            
            if (gameObject.activeSelf)
            {
                InitializeGrid();
            }
            else
            {
                CleanInventoryGrid();
            }
        }
    }
}

