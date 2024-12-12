using System;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryItem : MonoBehaviour
    {
        public Item Item;
        public Vector2Int GridPosition;
        [SerializeField] private bool rotated = false;
        public bool Rotated { get { return rotated; } }
        public RectTransform RectTransform { get; private set; }
        public int Width
        {
            get
            {
                if (!rotated)
                    return Item.Width;
                else
                    return Item.Height;
            }
        }
        public int Height
        {
            get
            {
                if (!rotated)
                    return Item.Height;
                else
                    return Item.Width;
            }
        }
        internal void Set(Item item)
        {
            Item = item;
            GetComponent<Image>().sprite = item.Sprite;

            RectTransform.sizeDelta = new(item.Width * ItemGrid.TileSizeWidth, item.Height * ItemGrid.TileSizeHeight);
        }

        void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        public void Rotate()
        {
            rotated = !rotated;

            float rotation = rotated == true ? -90f : 0;
            RectTransform.rotation = Quaternion.Euler(0, 0, rotation);
        }

    }
}

