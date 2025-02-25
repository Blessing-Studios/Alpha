using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;
using UnityEngine.VFX;

namespace Blessing.Gameplay.Characters.Traits
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Scriptable Objects/Traits/Buff")]
    public class Buff : Trait
    {
        public int Duration;
        [Tooltip("This buff will be called after the current buff ends")] 
        public Buff SideBuff;
    }
}