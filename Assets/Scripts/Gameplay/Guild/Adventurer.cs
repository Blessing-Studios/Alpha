using System.Collections.Generic;
using System.Linq;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Guild.Quests;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.Guild
{
    [RequireComponent(typeof(Character))]
    public class Adventurer : NetworkBehaviour
    {
        // Criar um sistema de jobs, a onde Adventurer é um job
        public Rank Rank;
        public List<int> QuestsCompleted = new();
        public List<int> QuestsActive = new();
        public Character Character;
        protected NetworkVariable<int> rankScore = new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected NetworkVariable<int> rankStrike = new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Tooltip("Quest the Adventurer is assigned to")]
        public List<Quest> Quests = new();
        void Awake()
        {
            Character = GetComponent<Character>();
        }

        public void Initialize(int rankScore, int rankStrike, List<int> questsCompleted, List<int> questsActive)
        {
            Rank = new Rank(rankScore, rankStrike);
            QuestsCompleted = questsCompleted;
            QuestsActive = questsActive;

            foreach (int questId in QuestsActive)
            {
                TakeQuest(questId);
            }
        }

        public override void OnNetworkSpawn()
        {
            GuildManager.Singleton.AddAdventurer(this);
        }

        public override void OnNetworkDespawn()
        {
            GuildManager.Singleton.RemoveAdventurer(this);
        }
        public void TakeQuest(int questId)
        {
            Quest quest = GuildManager.Singleton.Quests.First(s => s.Id == questId);
            if (quest != null)
            {
                AddQuest(quest);
            }
        }
        private void AddQuest(Quest quest)
        {
            if (Quests.Contains(quest)) return;
            quest.IsActive = true;
            Quests.Add(quest);

            if (QuestsActive.Contains(quest.Id)) return;
            QuestsActive.Add(quest.Id);
        }

        public bool IsQuestDone(int questId)
        {
            foreach(int id in QuestsCompleted)
            {
                if(id == questId) return true;
            }

            return false;
        }
    }
}
