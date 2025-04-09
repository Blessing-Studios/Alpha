using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Blessing.Gameplay.Characters
{
    [CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Characters/Archetype")]
    public class Archetype : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Label;
        public string Description;
        public Sprite Icon;
        public NetworkObject Prefab;
    }
}

