using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

// Muda Attributo ou recurso do jogador

// Lista dos tipos de Effects

// Muda Stats do character #
// Muda vida máxima do character
// Muda mana máxima do character #
// Muda vida regen do character
// Muda mana regen do character #
// Muda attack e defesa do character

namespace Blessing.Gameplay.Characters
{
    
    [Serializable] public struct StatChange
    {
        public Stat Stat;
        public int Value;
    }
    [Serializable]
    public struct HealthChange
    {
        public int Regen;
        public int Decay;
    }
    [Serializable] public struct ManaChange
    {
        public ManaColor ManaColor;
        public int Regen;
        public int Decay;
    }
    [Serializable] public struct AttackChange
    {
        public int Attack;
        public int Pen;
    }

    [Serializable] public struct DefenseChange
    {
        public int Defense;
        public int PenRes;
    }
    [CreateAssetMenu(fileName = "Effect", menuName = "Scriptable Objects/Characters/Effect")]
    [Serializable] public class Effect : ScriptableObject
    {
        public string Name;
        public string Label;
        [TextArea] public string Description;
        public StatChange[] StatChanges;
        public HealthChange[] HealthChanges;
        public ManaChange[] ManaChanges;
        public AttackChange[] AttackChanges;
        public DefenseChange[] DefenseChanges;
    }
}
