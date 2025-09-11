using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using Blessing.Player;
using Blessing.Gameplay.TradeAndInventory;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine.UIElements;

namespace Blessing.UI.PlayerHUD
{
    public class QuickUseSlot : ElementUi
    {
        public InventoryItem SelectedItem;
        public QuickUseItem SelectedQuickUseItem;
        public bool IsOpen = false;
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private InventoryGrid utilityGrid;
        [SerializeField] private Inventory utilityInventory;
        public RectTransform SliderRectTransform;
        [SerializeField] private float speed;
        [SerializeField] private float openWidth = 0;
        [SerializeField] private float closeWidth = 0;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private QuickUseItem quickUseItemPrefab;
        [SerializeField] private GameObject emptyBackGround;
        private List<QuickUseItem> quickUseItems= new();
        private const float itemWidth = 100;

        void Awake()
        {
            
        }

        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.I))
            // {
            //     UpdateQuickUseSlot();
            // }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            CloseSlot();
        }
        public void Initialize(InventoryGrid utilityGrid, Inventory utilityInventory)
        {
            this.utilityGrid = utilityGrid;
            this.utilityInventory = utilityInventory;

            UpdateQuickUseSlot();
        }
        public void Reset()
        {
            utilityGrid = null;
            utilityInventory = null;

            foreach (QuickUseItem quItem in quickUseItems)
            {
                quItem.Release();
            }
            quickUseItems.Clear();

            SelectedItem = null;

            if (SelectedQuickUseItem != null)
            {
                SelectedQuickUseItem.Release();
                SelectedQuickUseItem = null;
            }

            UpdateQuickUseSlot();
        }

        public void OpenSlot()
        {
            IsOpen = true;
            StopAllCoroutines();
            StartCoroutine(Open());
        }
        public void CloseSlot()
        {
            IsOpen = false;
            StopAllCoroutines();
            StartCoroutine(Close());
        }

        private void SelectQuickUseItem(InventoryItem selectedItem)
        {
            SelectedItem = selectedItem;

            if (SelectedQuickUseItem == null)
                SelectedQuickUseItem = PoolManager.Singleton.Get<QuickUseItem>(quickUseItemPrefab);

            SelectedQuickUseItem.Initialize(selectedItem, transform, false);


            SelectedQuickUseItem.Initialize(selectedItem, transform, true).Button.onClick.AddListener(() =>
            {
                if (IsOpen)
                    CloseSlot();
                else
                    OpenSlot();

                UIController.Singleton.PlayerHUD.SelectQuickSlot(this);
            });

            UIController.Singleton.PlayerHUD.SelectQuickSlot(this);
        }
        public void UseSelectedItem()
        {
            if (SelectedItem != null)
            {
                UIController.Singleton.HandleUseItem(SelectedItem, utilityGrid);
                UpdateQuickUseSlot();
            }
        }
        public void SelectQuickUseSlot()
        {
            if (SelectedQuickUseItem != null)
                SelectedQuickUseItem.Select(true);
        }
        public void UnSelectQuickUseSlot()
        {
            if (SelectedQuickUseItem != null)
                SelectedQuickUseItem.Select(false);
        }
        public void UpdateQuickUseSlot()
        {
            if (utilityInventory == null)
            {
                emptyBackGround.SetActive(true);
                return;
            }

            List<InventoryItem> items = utilityInventory.ItemList;

            if (SelectedItem == null && items.Count > 0)
            {
                SelectQuickUseItem(items[0]);
            }

            foreach (QuickUseItem quItem in quickUseItems)
            {
                quItem.Release();
            }
            quickUseItems.Clear();

            bool selectedItemFound = false;
            for (int i = 0; i < items.Count; i++)
            {
                bool selected = false;
                if (SelectedItem == items[i])
                {
                    selected = true;
                    selectedItemFound = true;
                }

                QuickUseItem quickUseItem = PoolManager.Singleton.Get<QuickUseItem>(quickUseItemPrefab).Initialize(items[i], itemsContainer, selected);
                quickUseItems.Add(quickUseItem);

                int itemIndex = i;

                quickUseItem.Button.onClick.AddListener(() =>
                {
                    foreach (QuickUseItem quItem in quickUseItems)
                    {
                        quItem.Select(false);
                    }

                    quickUseItem.Select(true);
                    SelectQuickUseItem(items[itemIndex]);
                    emptyBackGround.SetActive(false);
                });
            }

            openWidth = items.Count * itemWidth;

            // This will Update the Stack Text to show the item charge change
            if (selectedItemFound)
                SelectedQuickUseItem.UpdateItem();

            if (!selectedItemFound)
            {
                SelectedItem = null;
                if (SelectedQuickUseItem != null)
                {
                    SelectedQuickUseItem.Release();
                    SelectedQuickUseItem = null;
                }
            }

            if (!selectedItemFound && items.Count > 0)
            {
                SelectQuickUseItem(items[0]);
            }

            if (SelectedItem == null)
            {
                emptyBackGround.SetActive(true);
            }

            if (SelectedItem != null)
            {
                emptyBackGround.SetActive(false);
            }


        }

        private IEnumerator Open()
        {
            UpdateQuickUseSlot();
            
            float time = 0;
            float initialHeight = SliderRectTransform.sizeDelta.y;

            while (SliderRectTransform.sizeDelta.x < openWidth)
            {
                SliderRectTransform.sizeDelta = new Vector2(Mathf.Lerp(SliderRectTransform.sizeDelta.x, openWidth, time * speed), initialHeight);

                time += Time.deltaTime;
                yield return null;
            }

            StopAllCoroutines();
        }

        private IEnumerator Close()
        {
            float time = 0;
            float initialHeight = SliderRectTransform.sizeDelta.y;

            while (SliderRectTransform.sizeDelta.x >= closeWidth)
            {
                SliderRectTransform.sizeDelta = new Vector2(Mathf.Lerp(SliderRectTransform.sizeDelta.x, closeWidth, time * speed), initialHeight);

                time += Time.deltaTime;
                yield return null;
            }

            SliderRectTransform.sizeDelta = new Vector2(closeWidth, initialHeight);

            StopAllCoroutines();
        }
    }
}