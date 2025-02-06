using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    [Serializable]
    public struct InventoryItemData : IEquatable<InventoryItemData>, INetworkSerializeByMemcpy
    {
        public FixedString64Bytes Id;
        public int ItemId;
        public Vector2Int Position;
        public bool Rotated;

        public InventoryItemData(FixedString64Bytes id, int itemId, Vector2Int position, bool rotated)
        {
            Id = id;
            ItemId = itemId;
            Position = position;
            Rotated = rotated;
        }

        public bool Equals(InventoryItemData other)
        {
            return
                (this.Id == other.Id) &&
                (this.ItemId == other.ItemId) &&
                (this.Position == other.Position) &&
                (this.Rotated == other.Rotated);
        }
    }
    public class InventoryItem : MonoBehaviour
    {
        public Item Item;
        public InventoryItemData Data;
        public IObjectPool<InventoryItem> Pool;
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

            GetComponent<Image>().sprite = item.Sprite;

            gameObject.name = item.name;

            RectTransform.sizeDelta = new(item.Width * InventoryGrid.TileSizeWidth, item.Height * InventoryGrid.TileSizeHeight);
        }
        public void Set(Item item)
        {
            string stringGuid = Guid.NewGuid().ToString();
            Set(item, new InventoryItemData(new FixedString64Bytes(stringGuid), item.Id, Vector2Int.zero, false));
        }

        public void Set(Item item, InventoryItemData  data)
        {
            InitializeItem(item);
            SetData(data);
            item.Initialize(this);
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
        public void SetData(FixedString64Bytes id, int itemId, Vector2Int position, bool Rotated)
        {
            Data.Id = id;
            Data.ItemId = itemId;
            Data.Position = position;
            Data.Rotated = Rotated;
        }
    }
}

