using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Scriptable Objects/Skills/Projectile")]
    public class ProjectileSkill : Skill
    {
        [SerializeField] protected Projectile projectilePrefab;
        [field: SerializeField] public float Speed { get; protected set; }
        [field: SerializeField] public Vector2 PositionPrecision { get; protected set; }
        [Range(0, 90)] public float AnglePrecision;
        [field: SerializeField] public bool IsDestroyedOnHit { get; protected set; }
        [field: SerializeField] public float LifeTime { get; protected set; }
        public override void Trigger(ISkillTrigger skillTrigger)
        {
            base.Trigger(skillTrigger);
            
            Projectile projectile = PoolManager.Singleton.Get(projectilePrefab) as Projectile;
            projectile.Initialize(this, skillTrigger);
        }
    }
}