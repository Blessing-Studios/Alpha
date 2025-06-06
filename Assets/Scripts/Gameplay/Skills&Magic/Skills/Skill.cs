using System;
using System.Collections.Generic;
using Blessing.Audio;
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
        public Transform transform { get; }
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
        [TextArea] public string Description;
        public AudioClip[] AudioClips = new AudioClip[] {};
        public HitType HitType = HitType.Nothing;
        // public SkillType Type;
        [Tooltip("Damage and Pen of this skill will be applied in the next hit")]
        public bool ActiveSkillOnHit = false;
        // public SkillTrigger Trigger;
        public SkillModifier[] Modifiers;
        public int Attack = 0;
        public int DamageClass = 0;
        public float ImpactMultiplier = 1;
        public Buff[] Buffs;
        [field: SerializeField] public Skill AfterSkill { get; protected set; }
        [Tooltip("How much mana it will cost to use")] public ManaSpectrum ManaCost;

        public virtual void Trigger(ISkillTrigger skillTrigger, float randomFloat = 0.0f)
        {
            if (ActiveSkillOnHit)
                skillTrigger.ActiveSkill = this;

            if (AudioClips.Length > 0)
                AudioManager.Singleton.PlaySoundFx(AudioClips, skillTrigger.SkillOrigin);
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

        public virtual float GetSkillImpact(Dictionary<Stat, int> stats)
        {
            // O impacto da skill vai ser uma média ponderada dos SkillModifiers
            float skillImpact = 0;
            float totalModifier = 0;

            if (Modifiers.Length == 0) return 0;
            
            foreach (SkillModifier modifier in Modifiers)
            {
                totalModifier += modifier.Value;
                skillImpact += stats[modifier.Stat] * modifier.Value;
            }
            
            if (totalModifier == 0) return 0;

            skillImpact = skillImpact * ImpactMultiplier / totalModifier;

            return skillImpact;
        }
    }
}
