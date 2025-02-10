using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Scriptable Objects/Characters/Buff")]
    public class Buff : Trait
    {
        public float Duration;
    }
}

