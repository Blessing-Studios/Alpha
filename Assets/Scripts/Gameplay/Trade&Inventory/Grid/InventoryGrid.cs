using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryGrid : BaseGrid, IGrid
    {
        public Inventory Inventory;
        public TextMeshProUGUI NameText;

        public override void InitializeGrid()
        {
            if (Inventory == null)
            {
                gameObject.SetActive(false);
                return;
            }

            bool activated = false;
            if (!gameObject.activeSelf)
            {
                activated = true;
                gameObject.SetActive(true);
            }

            if (NameText != null)
                NameText.text = Inventory.Name;

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

            base.InitializeGrid();

            if (activated)
                gameObject.SetActive(false);
        }

        public void UpdateFromInventory()
        {   
            if (Inventory == null) 
            {
                return;
            }
            
            CleanGrid();

            // Refazer Lógica
            // Pega os itens do inventário e coloca no inventoryGrid
            foreach (InventoryItem item in Inventory.ItemList)
            {
                PlaceItemOnGrid(item, item.Data.Position);
            }
        }

        protected override void CleanGrid()
        {
            foreach (InventoryItem item in GridItems)
            {
                item.gameObject.SetActive(false);
            }

            GridItems.Clear();
        }
        public InventoryItem GetItem(Vector2Int position)
        {
            if (!PositionCheck(position)) return null;

            if (ShowDebug) Debug.Log(gameObject.name + ": ItemSlot - " + Inventory.ItemSlot[position.x, position.y]?.Item.name);
            return Inventory.ItemSlot[position.x, position.y];
        }
        public bool PlaceItem(InventoryItem inventoryItem)
        {
            Vector2Int? position = FindEmptyPosition(inventoryItem.Width, inventoryItem.Height);

            if (position == null) return false;

            return PlaceItem(inventoryItem, (Vector2Int) position);
        }
        public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            Inventory.GetOwnership();

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
            return Inventory.CheckAvailableSpace(position, width, height);
        }

        public InventoryItem PickUpItem(Vector2Int position)
        {
            Inventory.GetOwnership();

            InventoryItem inventoryItem = Inventory.ItemSlot[position.x, position.y];

            if (inventoryItem == null) return null;

            Inventory.RemoveItem(inventoryItem);

            GridItems.Remove(inventoryItem);

            return inventoryItem;
        }
        protected override bool BoundaryCheck(Vector2Int position, int width, int height)
        {
            return Inventory.BoundaryCheck(position, width, height);
        }

        protected override bool PositionCheck(Vector2Int position)
        {
            return Inventory.PositionCheck(position);
        }

        private Vector2Int? FindEmptyPosition(int width, int height)
        {
            return Inventory.FindEmptyPosition(width, height);
        }
    }
}
