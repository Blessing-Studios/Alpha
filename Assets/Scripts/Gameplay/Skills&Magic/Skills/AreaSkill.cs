using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Area", menuName = "Scriptable Objects/Skills/Area")]
    public class AreaSkill : Skill
    {
        public float Radius = 2.0f;
        public int MaxTargets = 4;
        public AreaEffect AreaEffect;

        public override void Trigger(ISkillTrigger skillTrigger)
        {
            AreaEffect areaEffect = PoolManager.Singleton.Get(AreaEffect) as AreaEffect;
            areaEffect.Initialize(this, skillTrigger);
        }
    }
}

