using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public abstract class BaseGrid : MonoBehaviour
    {
        public GameObject Owner;
        public const float TileSizeWidth = 46;
        public const float TileSizeHeight = 46;
        protected RectTransform rectTransform;
        [SerializeField] protected int gridSizeWidth = 20;
        [SerializeField] protected int gridSizeHeight = 10;
        public List<InventoryItem> GridItems;
        [field: SerializeField] public RectTransform TileFrame { get; protected set; }
        [field: SerializeField] public RectTransform ItemHighlight { get; protected set; }
        [field: SerializeField] public Vector2Int? HighlightPosition { get; protected set; } 
        [field: SerializeField] public bool IsOpen { get; protected set; }
        [SerializeField] protected NetworkObject looseItemPrefab;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            gameObject.SetActive(false);

            // TileSizeWidth = GetComponent<SpriteRenderer>().size.x;
            // TileSizeHeight = GetComponent<SpriteRenderer>().size.y;
        }

        public virtual void InitializeGrid()
        {
           SetTileFrame();
        }

        public virtual bool PlaceItemOnGrid(InventoryItem inventoryItem, Vector2Int position)
        {
            inventoryItem.RectTransform.SetParent(this.rectTransform);
            inventoryItem.RectTransform.SetAsLastSibling();

            inventoryItem.Data.Position = position;
            inventoryItem.RectTransform.localPosition = CalculatePosition(position, inventoryItem.Width, inventoryItem.Height);

            GridItems.Add(inventoryItem);

            inventoryItem.gameObject.SetActive(true);

            return true;
        }

        public virtual void RemoveItemFromGrid(InventoryItem inventoryItem)
        {
            GridItems.Remove(inventoryItem);
        }

        public virtual void SetHighlight(InventoryItem inventoryItem)
        {
            ItemHighlight.gameObject.SetActive(true);

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            // Set Highlight size
            ItemHighlight.sizeDelta = new Vector2() 
            {
                x = width * TileSizeWidth,
                y = height * TileSizeHeight
            };

            // Set Highlight position
            HighlightPosition = inventoryItem.Data.Position;
            ItemHighlight.localPosition = CalculatePosition(inventoryItem.Data.Position, width, height);
        }

        public virtual void SetTileFrame()
        {
            if (TileFrame == null) return;

            TileFrame.gameObject.SetActive(true);

            TileFrame.sizeDelta = new Vector2() 
            {
                x = gridSizeWidth * TileSizeWidth,
                y = gridSizeHeight * TileSizeHeight
            };
        }

        public virtual void SetHighlight(InventoryItem inventoryItem, Vector2Int position)
        {
            ItemHighlight.gameObject.SetActive(true);

            int width = inventoryItem.Width;
            int height = inventoryItem.Height;

            // Set Highlight size
            ItemHighlight.sizeDelta = new Vector2() 
            {
                x = width * TileSizeWidth,
                y = height * TileSizeHeight
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

        public virtual void RemoveHighlight()
        {
            HighlightPosition = null;
            ItemHighlight.gameObject.SetActive(false);
        }

        public virtual Vector2 CalculatePosition(Vector2Int position, int width, int height)
        {
            Vector2 localPosition = new()
            {
                x = position.x * TileSizeWidth + TileSizeWidth * width / 2,
                y = -(position.y * TileSizeHeight + TileSizeHeight * height / 2)
            };

            return localPosition;
        }

        public virtual Vector2Int GetTileGridPosition(Vector2 mousePosition)
        {
            Vector2 positionOnTheGrid = new();

            positionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
            positionOnTheGrid.y = rectTransform.position.y - mousePosition.y;

            Vector2Int tileGridPosition = new();
            
            tileGridPosition.x = (int)(positionOnTheGrid.x / TileSizeWidth);
            tileGridPosition.y = (int)(positionOnTheGrid.y / TileSizeWidth);

            return tileGridPosition;
        }

        public virtual void ToggleGrid()
        {   
            if (IsOpen)
            {
                CloseGrid();
            }
            else if (!IsOpen)
            {
                OpenGrid();
            }
        }

        public virtual void OpenGrid()
        {
            IsOpen = true;
            gameObject.SetActive(true);
            InitializeGrid();
        }

        public virtual void CloseGrid()
        {
            IsOpen = false;
            gameObject.SetActive(false);
            CleanGrid();
        }

        public virtual void ExitGrid()
        {
            RemoveHighlight();
        }

        protected abstract bool BoundaryCheck(Vector2Int position, int width, int height);
        protected abstract bool PositionCheck(Vector2Int position);
        protected abstract void CleanGrid();
    }
}