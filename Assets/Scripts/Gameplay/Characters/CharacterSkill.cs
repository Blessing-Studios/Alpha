using System;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [Serializable] public class CharacterSkill
    {
        public Skill Skill;
        public Vector2Int ComboMoveIndex;
        public bool IsActive;
    }
}
