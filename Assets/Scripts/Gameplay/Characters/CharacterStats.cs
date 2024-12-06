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
        [Tooltip("Measure physical power and carrying capacity")]
        [SerializeField]
        protected int strength;

        [Tooltip("Measuring endurance, stamina and max health")]
        [SerializeField]
        protected int constitution;

        [Tooltip("Measure agility, balance, coordination and reflexes")]
        [SerializeField]
        protected int dexterity;

        [Tooltip("Measure deductive reasoning, cognition, knowledge, memory, logic and rationality")]
        [SerializeField]
        protected int intelligence;

        [Tooltip("Measure self-awareness, common sense, restraint, perception and insight")]
        [SerializeField]
        protected int wisdom;

        [Tooltip("Measure force of personality, persuasiveness, leadership and successful planning")]
        [SerializeField]
        protected int charisma;

        [Tooltip("Measure force of luck, can influence the random events")]
        [SerializeField]
        protected int luck;

        [Space(10)]
        [Header("Original Character Stats")]
        [Space(10)]
        public bool UseBaseStats;
        [Header("Original Character Stats")]
        [Tooltip("Measure physical power and carrying capacity")]
        [field: SerializeField]
        public int BaseStrength { get; protected set; }

        [Tooltip("Measuring endurance, stamina and max health")]
        [field: SerializeField]
        public int BaseConstitution { get; protected set; }

        [Tooltip("Measure agility, balance, coordination and reflexes")]
        [field: SerializeField]
        public int BaseDexterity { get; protected set; }

        [Tooltip("Measure deductive reasoning, cognition, knowledge, memory, logic and rationality")]
        [field: SerializeField]
        public int BaseIntelligence { get; protected set; }

        [Tooltip("Measure self-awareness, common sense, restraint, perception and insight")]
        [field: SerializeField]
        public int BaseWisdom { get; protected set; }

        [Tooltip("Measure force of personality, persuasiveness, leadership and successful planning")]
        [field: SerializeField]
        public int BaseCharisma { get; protected set; }

        [Tooltip("Measure force of luck, can influence the random events")]
        [field: SerializeField]
        public int BaseLuck { get; protected set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void UpdateStat(Stat stat)
        {
            // TODO:
        }

        public void UpdateAllStats()
        {
            // TODO:
        }
    }
}