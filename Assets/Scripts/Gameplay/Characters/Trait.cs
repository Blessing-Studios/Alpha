using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Characters/Trait")]
    public class Trait : ScriptableObject
    {
        public string Name;
        public string Label;
        public string Description;
        public Effect[] Effects;

        // public int GetStatChange(Stat stat)
        // {
        //     int changeValue = 0;
        //     foreach (StatChange change in StatChanges)
        //     {
        //         if (change.Stat == stat)
        //         {
        //             changeValue += change.Value;
        //         }
        //     }

        //     return changeValue;
        // }

        public int GetStatChange(Stat stat)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (StatChange change in effect.StatChanges)
                if (change.Stat == stat)
                {
                    changeValue += change.Value;
                }
            }

            return changeValue;
        }

        public int GetHealthRegenChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (HealthChange change in effect.HealthChanges)
                {
                    changeValue += change.Regen;
                }
            }

            return changeValue;
        }
        public int GetHealthDecayChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (HealthChange change in effect.HealthChanges)
                {
                    changeValue += change.Decay;
                }
            }

            return changeValue;
        }

        public int GetAttackChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (AttackChange change in effect.AttackChanges)
                {
                    changeValue += change.Attack;
                }
            }

            return changeValue;
        }

        public int GetDefenseChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (DefenseChange change in effect.DefenseChanges)
                {
                    changeValue += change.Defense;
                }
            }

            return changeValue;
        }

        public int GetManaRegen(ManaColor color)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (ManaChange change in effect.ManaChanges)
                if (change.ManaColor == color)
                {
                    changeValue += change.Regen;
                }
            }

            return changeValue;
        }

        public int GetManaDecay(ManaColor color)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (ManaChange change in effect.ManaChanges)
                if (change.ManaColor == color)
                {
                    changeValue += change.Decay;
                }
            }

            return changeValue;
        }
    }
}

