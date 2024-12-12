using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class ItemGrid : MonoBehaviour
    {
        public const float TileSizeWidth = 32;
        public const float TileSizeHeight = 32;
        private RectTransform rectTransform;
        [SerializeField] private int gridSizeWidth = 20;
        [SerializeField] private int gridSizeHeight = 10;

        InventoryItem[,] inventoryItemSlot;

        [SerializeField] GameObject inventoryItemPrefab;
        [field: SerializeField] public RectTransform ItemHighlight { get; protected set; }
        [field: SerializeField] public Vector2Int? HighlightPosition { get; protected set; } 
        


        private Vector2 positionOnTheGrid = new();
        private Vector2Int tileGridPosition = new();

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Start()
        {
            InitializeGrid(gridSizeWidth, gridSizeHeight);
        }

        public void InitializeGrid(int width, int height)
        {   
            inventoryItemSlot = new InventoryItem[width, height];
            rectTransform.sizeDelta = new Vector2(width * TileSizeWidth, height * TileSizeHeight);
            ItemHighlight.SetParent(rectTransform);
            RemoveHighlight();

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

            return inventoryItemSlot[position.x, position.y];
        }
        public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            if (!CheckAvailableSpace(position, width, height)) return false;

            inventoryItem.RectTransform.SetParent(this.rectTransform);
            inventoryItem.RectTransform.SetAsLastSibling();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Criar um id ou hash no lugar de salvar o inventoryItem ref
                    inventoryItemSlot[position.x + x, position.y + y] = inventoryItem;
                }
            }

            inventoryItem.GridPosition = position;
            inventoryItem.RectTransform.localPosition = CalculatePosition(position, width, height);

            return true;
        }

        private bool CheckAvailableSpace(Vector2Int position, int width, int height)
        {
            if (!BoundaryCheck(position, width, height)) return false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (inventoryItemSlot[position.x + x, position.y + y] != null) return false;
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
            InventoryItem inventoryItem = inventoryItemSlot[position.x, position.y];

            if (inventoryItem == null) return null;

            for (int x = 0; x < inventoryItem.Width; x++)
            {
                for (int y = 0; y < inventoryItem.Height; y++)
                {
                    inventoryItemSlot[inventoryItem.GridPosition.x + x, inventoryItem.GridPosition.y + y] = null;
                }
            }

            return inventoryItem;
        }
        private bool BoundaryCheck(Vector2Int position, int width, int height)
        {
            if (!PositionCheck(position)) return false;

            position.x += width - 1;
            position.y += height - 1;

            if (!PositionCheck(position)) return false;

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
                x = width * ItemGrid.TileSizeWidth,
                y = height * ItemGrid.TileSizeHeight
            };

            // Set Highlight position
            HighlightPosition = inventoryItem.GridPosition;
            ItemHighlight.localPosition = CalculatePosition(inventoryItem.GridPosition, width, height);
        }

        public void SetHighlight(InventoryItem inventoryItem, Vector2Int position)
        {
            ItemHighlight.gameObject.SetActive(true);

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            // Set Highlight size
            ItemHighlight.sizeDelta = new Vector2() 
            {
                x = width * ItemGrid.TileSizeWidth,
                y = height * ItemGrid.TileSizeHeight
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
    }
}

