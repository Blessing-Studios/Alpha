using System;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryItem : MonoBehaviour
    {
        public Item Item;
        public InventoryItemData Data;
        public Inventory Inventory;
        public Vector2Int GridPosition { get { return Data.Position; } }
        public bool Rotated { get { return Data.Rotated; } }
        public int Value { get { return Item.Value; } }
        public RectTransform RectTransform { get; private set; }
        public int Width
        {
            get
            {
                if (!Data.Rotated)
                    return Item.Width;
                else
                    return Item.Height;
            }
        }
        public int Height
        {
            get
            {
                if (!Data.Rotated)
                    return Item.Height;
                else
                    return Item.Width;
            }
        }

        private void InitializeItem(Item item)
        {
            Item = item;
            Item.Initialize(this);

            GetComponent<Image>().sprite = item.Sprite;

            gameObject.name = item.name;

            RectTransform.sizeDelta = new(item.Width * InventoryGrid.TileSizeWidth, item.Height * InventoryGrid.TileSizeHeight);
        }
        public void Set(Item item)
        {
            InitializeItem(item);
            SetData(Guid.NewGuid(), item.Id, Vector2Int.zero, false);
        }

        public void Set(Item item, InventoryItemData  data)
        {
            InitializeItem(item);
            SetData(data);
        }

        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        public void Rotate()
        {
            Data.Rotated = !Data.Rotated;

            float rotation = Data.Rotated == true ? -90f : 0;
            RectTransform.rotation = Quaternion.Euler(0, 0, rotation);
        }

        public InventoryItemData GetData()
        {
            return Data; 
        }
        public void SetData(InventoryItemData data)
        {
            Data = data;

            // Deal with Rotation
            Data.Rotated = !Data.Rotated;
            Rotate();
        }
        public void SetData(Guid id, int itemId, Vector2Int position, bool Rotated)
        {
            Data.Id = id;
            Data.ItemId = itemId;
            Data.Position = position;
            Data.Rotated = Rotated;
        }
    }
}

