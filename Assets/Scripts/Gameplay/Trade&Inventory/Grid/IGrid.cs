using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public interface IGrid
    {
        public bool IsOpen { get; }
        // public GameObject Owner { get; }
        public RectTransform ItemHighlight { get; }
        public Vector2Int? HighlightPosition { get; }
        public void InitializeGrid();
        public InventoryItem GetItem(Vector2Int position);
        public bool PlaceItem(InventoryItem inventoryItem);
        public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position);
        public InventoryItem PickUpItem(Vector2Int position);
        public void SetHighlight(InventoryItem inventoryItem);
        public void SetHighlight(InventoryItem inventoryItem, Vector2Int position);
        public void RemoveHighlight();
        public Vector2Int GetTileGridPosition(Vector2 mousePosition);
        public void ToggleGrid();
        public void ExitGrid();
    }
}
