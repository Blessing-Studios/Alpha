using System.Collections.Generic;
using Blessing.Gameplay.Guild;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Gameplay.TradeAndInventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.Guild.Quests
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quests")]
    public class QuestScriptableObject : ScriptableObject
    {
        public int Id;
        public Rank Rank;
        public string Name;
        public string Label;
        public string Description;
        public bool IsCompleted;
        public bool IsActive;
        public KillObjective[] KillObjectives;
        public Item[] Rewards;

        public Quest Quest()
        {
            List<Objective> objectives = new();
            foreach( KillObjective objective in KillObjectives)
            {
                objectives.Add(new KillObjective(objective.Trophies, objective.Quantity));
            }
            
            return new Quest(Id, Rank, Name, Label, Description, objectives.ToArray(), Rewards);
        }
    }
}
