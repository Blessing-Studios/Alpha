using Blessing.Audio;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Area", menuName = "Scriptable Objects/Skills/Area")]
    public class AreaSkill : Skill
    {
        public float MinRadius = 2.0f;
        public int MaxTargets = 4;
        public AreaHit AreaHit;
        public bool CanSelfHit = false;
        public override void Trigger(ISkillTrigger skillTrigger, float randomFloat = 0.0f)
        {
            base.Trigger(skillTrigger);

            AreaHit areaHit = PoolManager.Singleton.Get(AreaHit) as AreaHit;
            areaHit.Initialize(this, skillTrigger);
        }
    }
}

