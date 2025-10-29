using System;
using Blessing.Gameplay.TradeAndInventory;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.Guild.Quests
{
    [Serializable]
    public struct Reward
    {
        public Item Item;
        public int Quantity;
    }
    [Serializable]
    public class Quest
    {
        public int Id { get { return QuestSO.Id;} }
        public Rank Rank;
        public string Name { get { return QuestSO.Name;} }
        public string Label { get { return QuestSO.Label;} }
        public string Description { get { return QuestSO.Description;} }
        public Sprite Icon { get { return QuestSO.Icon;} }
        public Sprite Banner { get { return QuestSO.Banner;} }
        public bool IsCompleted;
        public bool CanComplete;
        public bool IsActive;
        public Objective[] Objectives;
        public Reward[] Rewards;
        public QuestScriptableObject QuestSO;

        public Quest(Rank rank, Objective[] objectives, Reward[] rewards, QuestScriptableObject questSO)
        {
            Rank = rank;
            Objectives = objectives;
            Rewards = rewards;

            IsCompleted = false;
            CanComplete = false;
            IsActive = false;

            QuestSO = questSO;
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

            foreach (Reward reward in Rewards)
            {
                for (int i = 0; i < reward.Quantity; i++)
                {
                    InventoryItem inventoryItem = UIController.Singleton.CreateItem(reward.Item);

                    lootInventory.AddItem(inventoryItem);
                    inventoryItem.transform.SetParent(adventurer.transform, false);
                    inventoryItem.gameObject.SetActive(false);
                }
            }

            return true;
        }
    }
}

