using Blessing.Audio;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Beam", menuName = "Scriptable Objects/Skills/Beam")]
    public class BeamSkill : Skill
    {
        public float MaxDistance = 10f;
        public float hitsPerSecond = 1f;
        public int MaxTargets = 1;
        public bool CanSelfHit = false;
        public BeamHit BeamHit;
        public override void Trigger(ISkillTrigger skillTrigger, float randomFloat = 0.0f)
        {
            if (!skillTrigger.IsSkillHolding)
            {
                base.Trigger(skillTrigger);
                PoolManager.Singleton.Get<BeamHit>(BeamHit).Initialize(this, skillTrigger);
            }
        }
    }
}
