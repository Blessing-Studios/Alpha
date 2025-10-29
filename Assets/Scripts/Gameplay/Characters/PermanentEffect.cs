using System;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [Serializable]
    public struct StatPermanetChange
    {
        public Stat Stat;
        public int Value;
    }
    [Serializable]
    public struct HealthPermanentChange
    {
        public int Wound;
        public int Health;
    }
    [Serializable]
    public struct ManaPermanentChange
    {
        public ManaColor ManaColor;
        public int Value;
    }
    [CreateAssetMenu(fileName = "PermanentEffect", menuName = "Scriptable Objects/Characters/Permanentffect")]
    [Serializable]
    public class PermanentEffect : ScriptableObject
    {
        public string Name;
        public string Label;
        [TextArea] public string Description;
        public StatPermanetChange[] StatChanges;
        public HealthPermanentChange[] HealthChanges;
        public ManaPermanentChange[] ManaChanges;
    }
}
