using System;
using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Skills/Ability")]
    public class Ability : ScriptableObject
    {
        public Skill[] Skills;
        [SerializeField][ScriptableObjectDropdown(typeof(AnimationParamReference))] private ScriptableObjectReference animationParam;
        [SerializeField] public string AnimationParam { get { return (animationParam.value as AnimationParamReference).Name; } }
        [SerializeField][ScriptableObjectDropdown(typeof(AnimationParamReference))] private ScriptableObjectReference endAnimationParam;
        [SerializeField] public string EndAnimationParam { get { return (endAnimationParam.value as AnimationParamReference).Name; } }
        public bool CanCharge = false;
        public float CoolDown = 1.0f;
    }
}
