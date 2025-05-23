using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using Blessing.HealthAndDamage;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class Projectile : PooledObject, IHitter, ISkillTrigger
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] protected ProjectileSkill projectileSkill;
        [SerializeField] protected TrailRenderer[] trails;
        [SerializeField] protected HitEffect hitEffect;
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
        [SerializeField] protected Vector3 skillDirection = Vector3.right;
        public Vector3 SkillDirection { get { return skillDirection; }}
        public Dictionary<Stat, int> ValueByStat { get { return owner.ValueByStat;}}

        // Update is called once per frame
        void Update()
        {
            transform.position += speed * Time.deltaTime * direction;

            if (Timer >= lifeTime)
            {
                Release();
            }

            Timer += Time.deltaTime;
        }

        public Projectile Initialize(ProjectileSkill projectileSkill, ISkillTrigger owner, float precision)
        {
            this.projectileSkill = projectileSkill;
            this.owner = owner;
            speed = projectileSkill.Speed;
            lifeTime = projectileSkill.LifeTime;
            isDestroyedOnHit = projectileSkill.IsDestroyedOnHit;

            float rnd1 = Mathf.Floor(precision * 100);
            float rnd2 = Mathf.Floor(precision * 10000) - rnd1* 100;

            rnd1 = rnd1 / 100;
            rnd2 = rnd2 / 100;

            transform.SetPositionAndRotation(
                    owner.SkillOrigin.position + new Vector3(0, Mathf.Lerp(-projectileSkill.PositionPrecision.y, projectileSkill.PositionPrecision.y, rnd1), Mathf.Lerp(-projectileSkill.PositionPrecision.x, projectileSkill.PositionPrecision.x, rnd2)),
                    owner.SkillOrigin.rotation * Quaternion.Euler(0, Mathf.Lerp(-projectileSkill.AnglePrecision, projectileSkill.AnglePrecision, rnd1), Mathf.Lerp(-projectileSkill.AnglePrecision, projectileSkill.AnglePrecision, rnd2))
            );

            direction = transform.rotation * owner.SkillDirection;

            return this;
        }

        public bool Hit(IHittable target, Vector3 hitPosition)
        {
            if (!HasAuthority) return false;

            // TODO: create logic to not hit itself
            if (TargetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            target.GetOwnership();

            HitInfo = new HitInfo(projectileSkill.GetSkillDamage(owner.ValueByStat), projectileSkill.DamageClass, projectileSkill.GetSkillImpact(owner.ValueByStat), hitPosition, projectileSkill.Buffs);
            return true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!gameObject.activeSelf) return;

            if (other.gameObject.TryGetComponent(out HurtBox hurtBox))
            {
                Vector3 hitPosition = other.ClosestPoint(transform.position);
                // Set position.z to the target position z to simplify push
                hitPosition.y = hurtBox.Owner.transform.position.y;
                hitPosition.z = hurtBox.Owner.transform.position.z;

                if (Hit(hurtBox.Owner, hitPosition))
                {
                    // Pegar informação do dano e mantar para o target
                    hurtBox.GotHit(this);
                }
                if (isDestroyedOnHit) 
                {
                    projectileSkill.AfterSkill?.Trigger(this);

                    Release();
                }
            }
            else
            {
                if (isDestroyedOnHit) 
                {
                    
                    projectileSkill.AfterSkill?.Trigger(this);
                    Release();
                }
            }

            if (hitEffect != null)
            {
                HitEffect newHitEffect = PoolManager.Singleton.Get(hitEffect) as HitEffect;
                newHitEffect.transform.SetPositionAndRotation(other.ClosestPointOnBounds(transform.position), this.transform.rotation);
                newHitEffect.transform.position = other.ClosestPointOnBounds(transform.position);
                // newHitEffect.transform.SetParent(other.transform, true);
                newHitEffect.SetParentConstrain(other.transform);
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
