using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryController : MonoBehaviour
    {
        // This class needs to be in the MainCamera
        public ItemGrid SelectedItemGrid;
        [SerializeField] private List<Item> itemList = new();
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private InventoryItem selectedItem;
        
        void FixedUpdate()
        {
            HandleItemDrag();

            HandleHighlight();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                CreateRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                InsertRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateItem();
            }

            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseButtonPress();
            }
        }

        private void RotateItem()
        {
            if (selectedItem == null) return;

            selectedItem.Rotate();
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
            
            if (SelectedItemGrid == null) return;

            Vector2Int position = GetTileGridPosition();

            if (SelectedItemGrid.HighlightPosition == position) 
            {
                return;
            }

            if (selectedItem == null)
            {
                InventoryItem item = SelectedItemGrid.GetItem(position);
                if (item != null)
                {
                    SelectedItemGrid.SetHighlight(item);
                }
                else
                {
                    SelectedItemGrid.RemoveHighlight();
                }
            }
            else if (selectedItem != null)
            {
                SelectedItemGrid.SetHighlight(selectedItem, GetTileGridPosition());
            }
        }
        private void CreateRandomItem()
        {
            if (selectedItem != null) return;
            
            InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();

            inventoryItem.RectTransform.SetParent(canvasTransform);
            inventoryItem.Set(itemList[UnityEngine.Random.Range(0, itemList.Count)]);

            selectedItem = inventoryItem;
        }
        private void LeftMouseButtonPress()
        {
            if (SelectedItemGrid == null) return;

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
                position.x -= (selectedItem.Width -1) * ItemGrid.TileSizeWidth / 2;
                position.y += (selectedItem.Height - 1) * ItemGrid.TileSizeHeight / 2;
            }

            return SelectedItemGrid.GetTileGridPosition(position);
        }
        private void PlaceItem()
        {
            if (selectedItem == null) return;
            if (SelectedItemGrid == null) return;

            Vector2Int? position = SelectedItemGrid.FindEmptyPosition(selectedItem.Width, selectedItem.Height);

            if (position != null) 
                PlaceItem((Vector2Int) position);
        }
        private void PlaceItem(Vector2Int position)
        {
            if (selectedItem == null) return;
            if (SelectedItemGrid == null) return;

            if (SelectedItemGrid.PlaceItem(selectedItem, position))
                selectedItem = null;
        }

        private void PickUpItem(Vector2Int position)
        {
            selectedItem = SelectedItemGrid.PickUpItem(position);
        }
    }
}

