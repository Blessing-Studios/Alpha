using System;
using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Scriptable Objects/Skills/Ability")]
    public class Ability : ScriptableObject
    {
        public Skill[] Skills;
        [SerializeField][ScriptableObjectDropdown(typeof(AnimationParamReference))] private ScriptableObjectReference animationParam;
        public string AnimationParam { get { return (animationParam.value as AnimationParamReference).Name; } }
        [SerializeField][ScriptableObjectDropdown(typeof(AnimationParamReference))] private ScriptableObjectReference endAnimationParam;
        public string EndAnimationParam { get { return (endAnimationParam.value as AnimationParamReference).Name; } }
        public bool CanCharge = false;
        public float CoolDown = 1.0f;
        public Sprite IconSprite;
        public CameraShakeEffect ShakeEffect;
    }
}
