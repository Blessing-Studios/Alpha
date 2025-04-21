using System;
using System.Collections.Generic;
using System.Linq;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Gameplay.TradeAndInventory;
using UnityEngine;

namespace Blessing.Gameplay.Guild
{
    public class GuildManager : MonoBehaviour
    {
        public static GuildManager Singleton { get; private set; }
        public List<Adventurer> Adventurers = new();
        public List<QuestScriptableObject> QuestList = new();
        public List<Quest> Quests = new();
        void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Singleton = this;
            }
        }

        void Start()
        {
            foreach (QuestScriptableObject questSO in QuestList)
            {
                Quests.Add(questSO.Quest());
            }
        }

        public void AddAdventurer(Adventurer adventurer)
        {
            if (!Adventurers.Contains(adventurer))
                Adventurers.Add(adventurer);
        }

        public void RemoveAdventurer(Adventurer adventurer)
        {
            if (Adventurers.Contains(adventurer))
                Adventurers.Remove(adventurer);
        }

        public void TakeQuest(Adventurer adventurer, int questId)
        {
            adventurer.TakeQuest(questId);
        }

        public void CompleteQuest(Adventurer adventurer, int questId)
        {
            Quest quest = Quests.First(s => s.Id == questId);
            if (quest != null)
            {
                if (quest.Complete(adventurer))
                {
                    Debug.Log("Finished Quest with Id - " + quest.Id);
                }
                else
                {
                    Debug.Log("Failed to Finished Quest with Id - " + quest.Id);
                }

                // Give Reward to adventurer
                if (!quest.Complete(adventurer))
                {
                    // Cancela processo de completar quest
                    return;
                }

                if (!quest.GiveRewards(adventurer))
                {
                    // Cancela processo de completar quest
                    return;
                }

                // Update Adventurer Rank
                UpdateAdventurerRank(adventurer.Rank, quest.Rank);

                adventurer.Quests.Remove(quest);
                adventurer.QuestsCompleted.Add(quest.Id);

                return;
            }
        }

        private void UpdateAdventurerRank(Rank adventurerRank, Rank questRank, bool successfully = true)
        {
            if (successfully)
            {
                if (adventurerRank.Score <= questRank.Score && adventurerRank.Strike < Rank.MaxStrike)
                {
                    adventurerRank.AddStrikes(1);
                }

                if (adventurerRank.Score < questRank.Score && adventurerRank.Strike == Rank.MaxStrike)
                {
                    adventurerRank.RankUp();
                }
            }
            else
            {
                if (adventurerRank.Score >= questRank.Score && adventurerRank.Strike > Rank.MinStrike)
                {
                    adventurerRank.RemoveStrikes(1);
                }

                if (adventurerRank.Score > questRank.Score && adventurerRank.Strike == Rank.MinStrike)
                {
                    adventurerRank.RankDown();
                }
            }
        }
    }
}

