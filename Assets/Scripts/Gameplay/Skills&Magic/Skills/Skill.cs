using System;
using System.Collections.Generic;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using UnityEngine;
using UnityEngine.Pool;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public enum SkillType { Area, Hit, Projectile }
    public enum SkillTrigger { Instant, Cast, Passive }
    [Serializable] public struct SkillModifier
    {
        public Stat Stat;
        public int Value;
    }
    
    public interface ISkillTrigger
    {
        public bool HasAuthority { get; }
        public Skill ActiveSkill { get; set; }
        public Dictionary<Stat, int> ValueByStat { get; }
        public Transform SkillOrigin { get; }
        public Vector3 SkillDirection { get; }
    }
    [CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skills/Skill")]
    public class Skill : ScriptableObject
    {
        public string Name;
        public string Label;
        public string Description;
        public SkillType Type;
        // public SkillTrigger Trigger;
        public SkillModifier[] Modifiers;
        public int Attack = 0;
        public int DamageClass = 0;
        public Buff[] Buffs;
        [field: SerializeField] public Skill AfterSkill { get; protected set; }
        [Tooltip("How much mana it will cost to use")] public ManaSpectrum ManaCost;

        public virtual void Trigger(ISkillTrigger skillTrigger)
        {
            skillTrigger.ActiveSkill = this;
        }

        public virtual int GetSkillDamage(Dictionary<Stat, int> stats)
        {
            int skillDamage = Attack;

            if (Modifiers != null)
                foreach (SkillModifier modifier in Modifiers)
                {
                    skillDamage += stats[modifier.Stat] * modifier.Value;
                }

            return skillDamage;
        }
    }
}
