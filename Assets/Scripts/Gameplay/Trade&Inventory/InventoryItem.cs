using System;
using System.Collections;
using System.Data.Common;
using Blessing.Core.ObjectPooling;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    [Serializable]
    public struct  InventoryItemData : IEquatable<InventoryItemData>, INetworkSerializeByMemcpy
    {
        public FixedString64Bytes Id;
        public int ItemId;
        public Vector2Int Position;
        public bool Rotated;
        public int Stack;
        public InventoryItemData(FixedString64Bytes id, int itemId, Vector2Int position, bool rotated, int stack = 1)
        {
            Id = id;
            ItemId = itemId;
            Position = position;
            Rotated = rotated;
            Stack = stack;
        }

        public bool Equals(InventoryItemData other)
        {
            return
                Id == other.Id &&
                ItemId == other.ItemId &&
                Position == other.Position &&
                Rotated == other.Rotated && 
                Stack == other.Stack;
        }
    }
    public class InventoryItem : PooledObject
    {
        public Item Item;
        public InventoryItemData Data;
        public Inventory Inventory;
        public Vector2Int GridPosition { get { return Data.Position; } }
        public bool Rotated { get { return Data.Rotated; } }
        public int Value { get { return Item.Value; } }
        public RectTransform RectTransform { get; private set; }
        private Image image;
        [SerializeField] private TextMeshProUGUI stackText;
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
        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();

            if (stackText == null)
            {
                Debug.LogError(gameObject.name + ": stackText can't be null");
            }
        }
        private void InitializeItem(Item item)
        {
            Item = item;

            image.sprite = item.Sprite;

            gameObject.name = item.name;

            RectTransform.sizeDelta = new(item.Width * InventoryGrid.TileSizeWidth, item.Height * InventoryGrid.TileSizeHeight);
        }
        public void Set(Item item, int stack = 1)
        {
            string stringGuid = Guid.NewGuid().ToString();
            Set(item, new InventoryItemData(new FixedString64Bytes(stringGuid), item.Id, Vector2Int.zero, false, stack));
        }

        public void Set(Item item, InventoryItemData data)
        {
            InitializeItem(item);
            SetData(data);
            item.Initialize(this);
            UpdateStackNumber();
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

            // Reset Rotation
            Data.Rotated = !Data.Rotated;
            Rotate();
        }
        public void SetData(FixedString64Bytes id, int itemId, Vector2Int position, bool rotated, int stack)
        {
            Data.Id = id;
            Data.ItemId = itemId;
            Data.Position = position;
            Data.Rotated = rotated;
            Data.Stack = stack;
        }

        public void UpdateStackNumber()
        {
            if (Data.Stack > 1)
                stackText.text = $"{Data.Stack}";
            else
                stackText.text = "";
        }

        public void AddToStack(int qty)
        {
            if (qty <= 0) return;
            // teset
            Data.Stack += qty;
            UpdateStackNumber();
        }

        public void RemoveFromStack(int qty)
        {
            if (qty <= 0) return;

            Data.Stack -= qty;
            UpdateStackNumber();
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }

        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}

