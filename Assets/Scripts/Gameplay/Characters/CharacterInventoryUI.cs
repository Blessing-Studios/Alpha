using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Player;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(Canvas))]
    public class CharacterInventoryUI : MonoBehaviour
    {
        public Character Character;
        public IGrid SelectedGrid;
        public List<EquipmentSlot> Slots;
        public List<InventoryGrid> UtilityGrids;
        public InventoryGrid BackpackInventoryGrid;
        public CharacterStatsInfo CharacterStatsInfo;
        public Canvas Canvas { get; private set; }

        void Awake()
        {
            if (CharacterStatsInfo == null)
                TryGetComponent(out CharacterStatsInfo);

            Canvas = GetComponent<Canvas>();
        }
        public void SetCharacter(Character character)
        {
            Character = character;

            foreach (EquipmentSlot slot in Slots)
            {
                slot.Owner = character.gameObject;
            }

            CheckForDuplicatedEquipmentSlot();

            if (CharacterStatsInfo != null) CharacterStatsInfo.CharacterStats = character.Stats;
            
            SyncGrids();
        }

        private void CheckForDuplicatedEquipmentSlot()
        {

            Dictionary<string, int> qtyBySlotName = new();

            foreach(EquipmentSlot slot in Slots)
            {
                if (qtyBySlotName.ContainsKey(slot.GearSlotType.name))
                    qtyBySlotName[slot.GearSlotType.name]++;
                else
                    qtyBySlotName[slot.GearSlotType.name] = 0;

                slot.DuplicateIndex = qtyBySlotName[slot.GearSlotType.name];
            }
        }

        public void SetBackpackInventoryGrid(Inventory inventory = null)
        {
            if (inventory == null)
            {
                BackpackInventoryGrid.Inventory = null;
                return;
            }

            inventory.InventoryGrid = BackpackInventoryGrid;
            BackpackInventoryGrid.Inventory = inventory;
        }

        public void SetUtilityInventoryGrids(List<Inventory> utilityInventories)
        {
            for (int i = 0; i < UtilityGrids.Count; i++)
            {
                if (utilityInventories.Count > i && utilityInventories[i] != null)
                {
                    utilityInventories[i].InventoryGrid = UtilityGrids[i];
                    UtilityGrids[i].Inventory = utilityInventories[i];
                }
                else
                {
                    UtilityGrids[i].Inventory = null;
                }
            }
        }

        public void OpenInventoryUI()
        {
            gameObject.SetActive(true);

            foreach (EquipmentSlot slot in Slots)
            {
                slot.OpenGrid();
            }

            BackpackInventoryGrid.OpenGrid();

            for (int i = 0; i < UtilityGrids.Count; i++)
            {
                UtilityGrids[i].OpenGrid();
            }
        }

        public void CloseInventoryUI()
        {
            gameObject.SetActive(false);
        }
        public void SyncGrids()
        {
            if (Character == null) return;

            SetBackpackInventoryGrid(Character.Gear.Inventory);
            SetUtilityInventoryGrids(Character.Gear.UtilityInventories);

            foreach (EquipmentSlot slot in Slots)
            {
                slot.InitializeGrid();
            }

            BackpackInventoryGrid.InitializeGrid();

            for (int i = 0; i < UtilityGrids.Count; i++)
            {
                UtilityGrids[i].InitializeGrid();
            }

            if (CharacterStatsInfo != null) CharacterStatsInfo.UpdateStatInfo();
        }
    }
}
