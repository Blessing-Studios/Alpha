using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Core.GameEventSystem;
using System;
using UnityEngine;
using Blessing.Gameplay.Characters;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class EquipmentSlot : BaseGrid, IGrid
    {
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        public ScriptableObjectReference SlotType;
        [SerializeField] public EquipmentType GearSlotType { get { return SlotType.value as EquipmentType; } }
        public CharacterGear CharacterGear;
        public CharacterEquipment CharacterEquipment;
        public InventoryItem EquippedItem;

        [Header("Events")]
        public GameEvent OnAddItem;
        public GameEvent OnRemoveItem;
        public override void InitializeGrid()
        {
            bool activated = false;
            if (!gameObject.activeSelf)
            {
                activated = true;
                gameObject.SetActive(true);
            }

            gameObject.SetActive(true);

            rectTransform.sizeDelta = new Vector2(gridSizeWidth * TileSizeWidth, gridSizeHeight * TileSizeHeight);
            ItemHighlight.SetParent(rectTransform);
            RemoveHighlight();

            SetCharacterEquipment();

            if (EquippedItem != null)
                EquippedItem.gameObject.SetActive(true);

            base.InitializeGrid();

            if (activated)
                gameObject.SetActive(false);
        }

        private void SetCharacterEquipment()
        {
            if (Owner == null) return;

            if (Owner.TryGetComponent(out CharacterGear characterGear))
            {
                CharacterGear = characterGear;

                foreach (CharacterEquipment equipment in CharacterGear.Equipments)
                {
                    if (equipment.GearSlotType == GearSlotType)
                    {
                        CharacterEquipment = equipment;

                        EquippedItem = CharacterEquipment.InventoryItem;
                        if (EquippedItem != null)
                            PlaceItemOnGrid(EquippedItem, Vector2Int.zero);
                            
                        return;
                    }
                }
            }
        }

        public InventoryItem GetItem(Vector2Int position)
        {
            return EquippedItem;
        }
        public bool PlaceItem(InventoryItem inventoryItem)
        {
            Vector2Int? position = FindEmptyPosition();

            if (position == null) return false;

            return PlaceItem(inventoryItem, (Vector2Int) position);
        }

        public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            // Mudar quem tem autoridade para poder fazer a mudança online

            if (!CheckGearType(inventoryItem))
                return false;
            
            if (!CheckAvailableSpace())
                return false;

            if (!BoundaryCheck(position, inventoryItem.Width, inventoryItem.Height))
                return false;

            //Rotate Item to the right side
            if (inventoryItem.Rotated)
                inventoryItem.Rotate();

            // Move the item in the right position on the InventoryGrid
            if (!PlaceItemOnGrid(inventoryItem, position))
                return false;

            // Equip Item
            if (!CharacterGear.AddEquipment(CharacterEquipment, inventoryItem))
                return false;

            // if (!CharacterEquipment.SetEquipment(inventoryItem))
            //     return false;

            EquippedItem = inventoryItem;

            // Raise Events
            if (OnAddItem != null)
                OnAddItem.Raise(this, inventoryItem);

            return true;
        }

        private bool CheckGearType(InventoryItem inventoryItem)
        {
            Gear item = inventoryItem.Item as Gear;

            if (item == null) return false;

            if (GearSlotType != item.GearType) return false;

            return true;

        }

        private bool CheckAvailableSpace()
        {
            if (EquippedItem != null) return false;

            return true;
        }

        private Vector2Int? FindEmptyPosition()
        {
            if (EquippedItem != null) return null;

            return Vector2Int.zero;
        }
        public InventoryItem PickUpItem(Vector2Int position)
        {
            InventoryItem inventoryItem = EquippedItem;

            if (EquippedItem == null) return null;

            EquippedItem = null;

            RemoveItemFromGrid(inventoryItem);

            CharacterGear.RemoveEquipment(CharacterEquipment, inventoryItem);

            // Raise Event
            if (OnRemoveItem != null)
                OnRemoveItem.Raise(this, inventoryItem);

            return inventoryItem;
        }

        protected override void CleanGrid()
        {
            InventoryItem item = EquippedItem;
            if (item == null) return;

            item.gameObject.SetActive(false);

            EquippedItem = null;
        }

        protected override bool BoundaryCheck(Vector2Int position, int width, int height)
        {
            if (!PositionCheck(position))
            {
                return false;
            }

            position.x += width - 1;
            position.y += height - 1;

            if (!PositionCheck(position))
            { 
                return false;
            }

            return true;
        }

        protected override bool PositionCheck(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0) return false;

            if (position.x >= gridSizeWidth || position.y >= gridSizeHeight) return false;

            return true;
        }
    }
}

