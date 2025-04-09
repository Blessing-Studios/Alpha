using System.Collections.Generic;
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
        public Character Character;
        protected NetworkVariable<int> rankScore = new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected NetworkVariable<int> rankStrike = new NetworkVariable<int>(0,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public List<Quest> Quests = new();
        void Awake()
        {
            Character = GetComponent<Character>();
        }

        public void Initialize(int rankScore, int rankStrike, List<int> questsCompleted)
        {
            Rank = new Rank(rankScore, rankStrike);
            QuestsCompleted = questsCompleted;
        }

        public override void OnNetworkSpawn()
        {
            GuildManager.Singleton.AddAdventurer(this);
        }

        public override void OnNetworkDespawn()
        {
            GuildManager.Singleton.RemoveAdventurer(this);
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
