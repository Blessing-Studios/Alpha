using System;
using Blessing.Gameplay.SkillsAndMagic;
using Blessing.Gameplay.Characters.InputActions;
using UnityEngine.UI;
using UnityEngine;
using Blessing.Gameplay.Characters.Traits;

namespace Blessing.Gameplay.Characters
{
    [Serializable]
    public class CharacterAbility
    {
        public Ability Ability;
        public float CoolDownTimer = 0;
        public CastActionType CastAction { get; set; }
        public Skill[] Skills { get { return Ability.Skills; } }
        public Buff[] Buffs { get { return Ability.Buffs; }}
        public Sprite IconSprite { get { return Ability.IconSprite; } }
        public CameraShakeEffect ShakeEffect { get { return Ability.ShakeEffect; } }
        public bool CanMove { get { return Ability.CanMove; } }
        public bool CanFace { get { return Ability.CanFace; } }
        public bool CanTriggerOnHold { get { return Ability.CanTriggerOnHold; } }
        public float SpeedMultiplayer { get { return Ability.SpeedMultiplayer; }}
        public CharacterAbility(Ability ability, CastActionType castAction)
        {
            Ability = ability;
            CastAction = castAction;
            CoolDownTimer = 0;
        }
    }
}
