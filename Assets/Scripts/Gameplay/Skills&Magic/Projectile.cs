using System;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using Blessing.HealthAndDamage;
using UnityEngine;
using UnityEngine.Pool;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class Projectile : PooledObject, IHitter, ISkillTrigger
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] protected ProjectileSkill projectileSkill;
        [SerializeField] protected TrailRenderer[] trails;
        [SerializeField] protected float speed;
        [SerializeField] protected float lifeTime;
        [SerializeField] protected bool isDestroyedOnHit;
        public float Timer;
        [SerializeField] protected Vector3 direction;
        [SerializeField] protected ISkillTrigger owner;
        [SerializeField] public bool HasAuthority { get { return owner.HasAuthority;} }
        public List<IHittable> TargetList = new();
        public HitInfo HitInfo { get; protected set; }
        public Skill ActiveSkill { get; set; }
        public Transform SkillOrigin { get { return transform; } }
        protected Vector3 skillDirection = Vector3.right;
        public Vector3 SkillDirection { get { return skillDirection; }}
        public Dictionary<Stat, int> ValueByStat { get { return owner.ValueByStat;}}

        // Update is called once per frame
        void Update()
        {
            transform.position += speed * Time.deltaTime * direction;

            if (Timer >= lifeTime)
            {
                Pool.Release(this);
            }

            Timer += Time.deltaTime;
        }

        public Projectile Initialize(ProjectileSkill projectileSkill, ISkillTrigger owner)
        {
            this.projectileSkill = projectileSkill;
            this.owner = owner;
            speed = projectileSkill.Speed;
            lifeTime = projectileSkill.LifeTime;
            isDestroyedOnHit = projectileSkill.IsDestroyedOnHit;
            transform.SetPositionAndRotation(owner.SkillOrigin.position, owner.SkillOrigin.rotation);
            direction = transform.rotation * owner.SkillDirection;

            return this;
        }

        public bool Hit(IHittable target)
        {
            if (!HasAuthority) return false;

            // TODO: create logic to not hit itself

            if (TargetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            target.GetOwnership();

            HitInfo = new HitInfo(projectileSkill.GetSkillDamage(owner.ValueByStat), projectileSkill.DamageClass, 0f, projectileSkill.Buffs);
            return true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeSelf) return;

            Debug.Log(gameObject.name + ": OnCollisionEnter");

            if (other.gameObject.TryGetComponent(out HurtBox hurtBox))
            {
                if (Hit(hurtBox.Owner))
                {
                    // Pegar informação do dano e mantar para o target
                    hurtBox.Owner.GotHit(this);
                }
                if (isDestroyedOnHit) 
                {
                    projectileSkill.AfterSkill.Trigger(this);

                    Pool.Release(this);
                }
            }
            else
            {
                if (isDestroyedOnHit) 
                {
                    projectileSkill.AfterSkill.Trigger(this);

                    Pool.Release(this);
                }
            }
        }
        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }
        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            TargetList.Clear();
            Timer = 0.0f;
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }

        void OnDisable()
        {
            foreach (TrailRenderer trail in trails)
            {
                trail.Clear();
            }           
        }
    }
}
