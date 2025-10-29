using Blessing.Core.ObjectPooling;
using Blessing.HealthAndDamage;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
using System.Collections.Generic;
using Blessing.Audio;

namespace Blessing.Gameplay.SkillsAndMagic
{
    // AreaHit Não pode ser um ScriptableObject porque ela term que ter a interface IHitter
    // TODO: Criar Classe SkillEffect para ser o pai da AreaEffect
    public class AreaHit : PooledObject, IHitter
    {
        [SerializeField] protected AreaSkill areaSkill;
        [SerializeField] protected ISkillTrigger owner;
        [SerializeField] protected List<IHittable> targets = new();
        [SerializeField] public bool HasAuthority { get { return owner.HasAuthority; } }
        public HitInfo HitInfo { get; protected set; }
        [SerializeField] protected Collider[] colliders = new Collider[100];
        [SerializeField] protected ParticleSystem particleEffect;
        [SerializeField] protected VisualEffect visualEffect;
        [SerializeField] protected AudioClip[] audioClips;
        private float timer;
        private float duration;

        public void Initialize(AreaSkill areaSkill, ISkillTrigger owner)
        {
            this.areaSkill = areaSkill;
            this.owner = owner;

            transform.position = owner.SkillOrigin.position;

            targets.Clear();
            timer = 0;

            int numberOfHits = 0;
            int hitsNumber = Physics.OverlapSphereNonAlloc(transform.position, areaSkill.MinRadius, colliders, LayerMask.GetMask("HurtBoxes"));

            IEnumerable<Collider> orderedColliders = colliders.OrderBy(c => c != null ? (c.transform.position - transform.position).sqrMagnitude : int.MaxValue).Take(hitsNumber);

            foreach (Collider collider in orderedColliders)
            {
                // Handle Max Targets hit
                if (numberOfHits >= areaSkill.MaxTargets) break;

                if (collider.gameObject.TryGetComponent(out HurtBox hurtBox))
                {
                    // Handle interact with itself
                    if (hurtBox.Owner.transform == owner.transform && !areaSkill.CanSelfHit) continue;

                    // Check if already hit target
                    if (targets.Contains(hurtBox.Owner)) continue;

                    Debug.Log("Teste Hit " + hurtBox.Owner.transform.gameObject.name + ": Distance - " + (collider.transform.position - transform.position).sqrMagnitude);

                    if (Hit(hurtBox.Owner, collider.ClosestPoint(transform.position)))
                    {
                        // Pegar informação do dano e mantar para o target
                        hurtBox.GotHit(this);

                        targets.Add(hurtBox.Owner);
                        numberOfHits++;
                    }
                }
            }



            float vEDuration = visualEffect != null ? visualEffect.GetFloat("LifeTime") : 0;
            float pEDuration = particleEffect != null ? particleEffect.main.duration : 0;

            duration = vEDuration > pEDuration ? vEDuration : pEDuration;
            
            if (audioClips.Length > 0)
                AudioManager.Singleton.PlaySoundFx(audioClips, transform);
        }

        void Update()
        {
            if (timer >= duration)
            {
                Pool.Release(this);
            }

            timer += Time.deltaTime;
        }

        public bool Hit(IHittable target, Vector3 hitPosition)
        {
            if (!HasAuthority) return false;

            // TODO: create logic to not hit itself

            target.GetOwnership();

            HitInfo = new HitInfo(areaSkill.GetSkillDamage(owner.ValueByStat), areaSkill.DamageClass, areaSkill.GetSkillImpact(owner.ValueByStat), hitPosition, areaSkill.Buffs, areaSkill.HitType);
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
