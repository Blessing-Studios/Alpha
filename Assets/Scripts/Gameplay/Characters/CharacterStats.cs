using System;
using System.Collections.Generic;
using Blessing.Core.GameEventSystem;
using Unity.Properties;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    public enum Stat 
    {
        Strength,
        Constitution,
        Dexterity,
        Intelligence,
        Wisdom,
        Charisma,
        Luck
    }
    public class CharacterStats : MonoBehaviour
    {
        public int Attack;

        [Header("Character Physical and Mental Attributes")]
        [Space(10)]
        [Tooltip("Measure physical power and carrying capacity")]
        public int Strength;

        [Tooltip("Measuring endurance, stamina and max health")]
        public int Constitution;

        [Tooltip("Measure agility, balance, coordination and reflexes")]
        public int Dexterity;

        [Tooltip("Measure deductive reasoning, cognition, knowledge, memory, logic and rationality")]
        public int Intelligence;

        [Tooltip("Measure self-awareness, common sense, restraint, perception and insight")]
        public int Wisdom;

        [Tooltip("Measure force of personality, persuasiveness, leadership and successful planning")]
        public int Charisma;

        [Tooltip("Measure force of luck, can influence the random events")]
        public int Luck;

        [Space(10)]
        [Header("Original Character Stats")]
        [Space(10)]
        // public bool UseBaseStats;
        [Header("Original Character Stats")]
        [Tooltip("Measure physical power and carrying capacity")]
        public int BaseStrength;

        [Tooltip("Measuring endurance, stamina and max health")]
        public int BaseConstitution;

        [Tooltip("Measure agility, balance, coordination and reflexes")]
        public int BaseDexterity;

        [Tooltip("Measure deductive reasoning, cognition, knowledge, memory, logic and rationality")]
        public int BaseIntelligence;

        [Tooltip("Measure self-awareness, common sense, restraint, perception and insight")]
        public int BaseWisdom;

        [Tooltip("Measure force of personality, persuasiveness, leadership and successful planning")]
        public int BaseCharisma;
        [Tooltip("Measure force of luck, can influence the random events")]
        public int BaseLuck;
        public Trait[] Traits;

        [Header("Events")]
        public GameEvent OnUpdateAllStats;
        public GameEvent OnUpdateStat;

        void Start()
        {
            // UpdateAllStats();
        }
        public int GetStatValue(Stat stat)
        {
            return (int) GetType().GetField(stat.ToString()).GetValue(this);
        }

        public void UpdateAllStats(List<Trait> traits)
        {
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                UpdateStat(stat, traits);
            }

            // Raise Events
            if (OnUpdateAllStats != null)
                OnUpdateAllStats.Raise(this);
        }

        public void UpdateStat(Stat stat, List<Trait> traits)
        {
            int statValue = (int) GetType().GetField("Base" + stat.ToString()).GetValue(this);

            foreach (Trait trait in traits)
            {
                statValue += trait.GetStatChange(stat);
            }

            GetType().GetField(stat.ToString()).SetValue(this, statValue);

            // Raise Events
            if (OnUpdateStat != null)
                OnUpdateStat.Raise(this, stat);
        }
    }
}