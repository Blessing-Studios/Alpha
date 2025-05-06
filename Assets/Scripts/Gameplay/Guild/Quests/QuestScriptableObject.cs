using System.Collections.Generic;
using Blessing.Gameplay.Guild;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Gameplay.TradeAndInventory;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.Guild.Quests
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Scriptable Objects/Quests")]
    public class QuestScriptableObject : ScriptableObject
    {
        public int Id;
        public int RankScore;
        public int RankStrike;
        public string Name;
        public string Label;
        [TextArea] public string Description;
        public Sprite Icon;
        public Sprite Banner;
        public KillObjective[] KillObjectives;
        public Reward[] Rewards;

        public Quest Quest()
        {
            List<Objective> objectives = new();
            foreach( KillObjective objective in KillObjectives)
            {
                objectives.Add(new KillObjective(objective.Trophies, objective.Quantity));
            }
            
            return new Quest(new Rank(RankScore, RankStrike), objectives.ToArray(), Rewards, this);
        }
    }
}
