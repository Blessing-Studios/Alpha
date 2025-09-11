using System.Collections.Generic;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.UI.Quests;
using UnityEngine;

namespace Blessing.Gameplay.Guild.Quests
{
    public class QuestBoard : MonoBehaviour, IInteractable
    {
        public bool CanInteract { get { return true; } }

        public void Interact(Interactor interactor)
        {
            Adventurer adventurer = interactor.GetComponent<Adventurer>();

            if (adventurer == null) return;

            UIController.Singleton.OpenQuestsUI(ListAllQuests(), adventurer);
        }

        // TODO: Add validations to get Task
        public List<Quest> ListAllQuests()
        {
            return GuildManager.Singleton.Quests;
        }

        public List<Quest> ListQuestsByRank(Rank rank)
        {
            List<Quest> quests = new();

            foreach (Quest quest in GuildManager.Singleton.Quests)
            {
                if(quest.Rank.Score == rank.Score || (quest.Rank.Score == (rank.Score - 1) && rank.Strike == 3))
                {
                    quests.Add(quest);
                    continue;
                }
            }

            return quests;
        }

        public void TakeQuest(Adventurer adventurer, int questId)
        {
            GuildManager.Singleton.TakeQuest(adventurer, questId);
        }
    }
}
