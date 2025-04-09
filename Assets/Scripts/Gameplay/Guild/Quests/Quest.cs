using System;
using Blessing.Gameplay.TradeAndInventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.Guild.Quests
{
    [Serializable]
    public class Quest
    {
        public int Id;
        public Rank Rank;
        public string Name;
        public string Label;
        public string Description;
        public bool IsCompleted;
        public bool CanComplete;
        public bool IsActive;
        public Objective[] Objectives;
        public Item[] Rewards;

        public Quest(int id, Rank rank, string name, string label, string description, Objective[] objectives, Item[] rewards)
        {
            Id = id;
            Rank = rank;
            Name = name;
            Label = label;
            Description = description;
            Objectives = objectives;
            Rewards = rewards;

            IsCompleted = false;
            CanComplete = false;
            IsActive = false;
        }

        public bool Validate(Adventurer adventurer)
        {
            CanComplete = true;

            // Validate Quest Objectives
            foreach (Objective objective in Objectives)
            {
                if (!objective.Validate(adventurer))
                {
                    CanComplete = false;
                }
            }

            return CanComplete;
        }

        public bool Complete(Adventurer adventurer)
        {
            IsCompleted = true;

            foreach (Objective objective in Objectives)
            {
                if (!objective.Complete(adventurer))
                {
                    IsCompleted = false;
                }
            }

            return IsCompleted;
        }

        public bool GiveRewards(Adventurer adventurer)
        {
            Inventory lootInventory = adventurer.Character.Gear.Inventory;
            if (lootInventory == null)
            { 
                return false; 
            }

            foreach (Item item in Rewards)
            {
                InventoryItem inventoryItem = GameManager.Singleton.GetInventoryItem();
                inventoryItem.Set(item);

                lootInventory.AddItem(inventoryItem);
                inventoryItem.transform.SetParent(adventurer.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }

            return true;
        }
    }
}

