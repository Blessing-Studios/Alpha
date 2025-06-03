using System;
using Blessing.Gameplay.SkillsAndMagic;
using Blessing.Gameplay.Characters.InputActions;
using UnityEngine.UI;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [Serializable]
    public class CharacterAbility
    {
        public Ability Ability;
        public float CoolDownTimer = 0;
        public CastActionType CastAction { get; set; }
        public Skill[] Skills { get { return Ability.Skills; } }
        public Sprite IconSprite { get { return Ability.IconSprite; } }
        public CameraShakeEffect ShakeEffect { get { return Ability.ShakeEffect; } } 
        public CharacterAbility(Ability ability, CastActionType castAction)
        {
            Ability = ability;
            CastAction = castAction;
            CoolDownTimer = 0;
        }
    }
}
