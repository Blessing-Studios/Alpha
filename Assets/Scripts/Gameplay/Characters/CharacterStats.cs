using System;
using System.Collections.Generic;
using Blessing.Gameplay.Characters.Traits;
using UnityEngine;

namespace Blessing.Characters
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
        [Header("Character Physical and Mental Attributes")]
        [Space(10)]
        private bool test123;
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
        public bool UseBaseStats;
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
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        [Header("Character Traits")]
        public List<Trait> Traits;

        void Start()
        {
            UpdateAllStats();
        }

        public void UpdateStat(Stat stat)
        {
            
            int statValue = (int) GetType().GetField("Base" + stat.ToString()).GetValue(this);

            foreach (Trait trait in Traits)
            {
                statValue += trait.GetStatChange(stat);
            }

            GetType().GetField(stat.ToString()).SetValue(this, statValue);
        }

        public void UpdateAllStats()
        {
            Debug.Log("UpdateALl Entrou");
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                UpdateStat(stat);
            }
        }

        public bool AddTrait(Trait trait)
        {
            // Check if Trait can be added

            Traits.Add(trait);
            return true;
        }

        public bool RemoveTrait(Trait trait)
        {
            // Check if Trait can be removed
            
            Traits.Remove(trait);
            return true;
        }

    }
}