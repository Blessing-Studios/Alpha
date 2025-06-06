using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Passive", menuName = "Scriptable Objects/Skills/Passive")]
    public class PassiveSkill : Skill
    {
        [Tooltip("How much mana it will cost by tick")] public ManaSpectrum PassiveManaCost;
        public override void Trigger(ISkillTrigger skillTrigger, float randomFloat = 0.0f)
        {
            base.Trigger(skillTrigger);
        }
    }
}