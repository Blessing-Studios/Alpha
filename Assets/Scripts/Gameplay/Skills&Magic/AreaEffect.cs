using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.HealthAndDamage;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class AreaEffect : PooledObject, IHitter
    {
        [SerializeField] protected AreaSkill areaSkill;
        [SerializeField] protected ISkillTrigger owner;
        [SerializeField] public bool HasAuthority { get { return owner.HasAuthority; } }
        public HitInfo HitInfo { get; protected set; }
        [SerializeField] protected Collider[] colliders = new Collider[50];
        [SerializeField] protected ParticleSystem explosion;
        private float timer;
        private float duration;

        public void Initialize(AreaSkill areaSkill, ISkillTrigger owner)
        {
            this.areaSkill = areaSkill;
            this.owner = owner;

            transform.position = owner.SkillOrigin.position;
            
            int numberOfHits = 0;
            int hitsNumber = Physics.OverlapSphereNonAlloc(transform.position, areaSkill.Radius, colliders);
            for (int i = 0; i < hitsNumber; i++)
            {
                // Can't interact with itself
                if (owner.SkillOrigin == colliders[i].transform) continue;

                if (colliders[i].gameObject.TryGetComponent(out HurtBox hurtBox))
                {
                    if (Hit(hurtBox.Owner))
                    {
                        // Pegar informação do dano e mantar para o target
                        hurtBox.Owner.GotHit(this);

                        numberOfHits++;
                    }
                }

                if (numberOfHits > areaSkill.MaxTargets) break;
            }

            timer = 0;
            duration = explosion.main.duration;
        }

        void Update()
        {
            if (timer >= duration)
            {
                Pool.Release(this);
            }

            timer += Time.deltaTime;
        }

        public bool Hit(IHittable target)
        {
            if (!HasAuthority) return false;

            // TODO: create logic to not hit itself

            target.GetOwnership();

            HitInfo = new HitInfo(areaSkill.GetSkillDamage(owner.ValueByStat), areaSkill.DamageClass, areaSkill.Buffs);
            return true;
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }

        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
