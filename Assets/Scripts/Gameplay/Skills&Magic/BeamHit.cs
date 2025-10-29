using Blessing.Core.ObjectPooling;
using Blessing.HealthAndDamage;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
using System.Collections.Generic;
using Blessing.Audio;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public class BeamHit : PooledObject, IHitter
    {
        [SerializeField] protected BeamSkill beamSkill;
        [SerializeField] protected ISkillTrigger owner;
        [SerializeField] protected List<IHittable> targets = new();
        [SerializeField] public bool HasAuthority { get { return owner.HasAuthority; } }
        public HitInfo HitInfo { get; protected set; }
        [SerializeField] protected ParticleSystem particleEffect;
        [SerializeField] protected VisualEffect visualEffect;
        [SerializeField] protected AudioClip[] audioClips;
        private RaycastHit[] hits = new RaycastHit[10];
        [SerializeField] private LineRenderer lineRenderer;
        private Vector3 beamDirection = Vector3.right;
        private int maxTargets;
        private float timer;
        private float duration;
        private bool hasHitTimeReached;
        private float hitTime;
        public void Initialize(BeamSkill beamSkill, ISkillTrigger owner)
        {
            this.beamSkill = beamSkill;
            this.owner = owner;
            maxTargets = beamSkill.MaxTargets;
            
            hitTime = beamSkill.hitsPerSecond > 0.1 ? 1 / beamSkill.hitsPerSecond : 1;

            targets.Clear();
            timer = 0;
            hasHitTimeReached = false;

            transform.SetPositionAndRotation(
                    owner.SkillOrigin.position,
                    owner.SkillOrigin.rotation
            );

            // transform.SetParent(owner.transform, false);

            gameObject.SetActive(true);

            // Calcular a direção do raio usando lógica do sensor de direção
            beamDirection = owner.transform.right;
        }
        void Update()
        {
            timer += Time.deltaTime;

            if (!owner.IsSkillHolding && hasHitTimeReached)
            {
                // trigger after skill and release to pool
                Debug.Log("BeamHit Exit: ");
                Release();
                return;
            }

            Debug.Log("BeamHit 0: ");

            List<RaycastHit> hitsHurtBoxes = new();
            Dictionary<RaycastHit, HurtBox> dicHurtBoxByHit = new();

            int numHits = Physics.RaycastNonAlloc(new Ray(owner.SkillOrigin.position, owner.transform.right), hits, beamSkill.MaxDistance);

            if (numHits > 0)
            {
                for (int i = 0; i < numHits; i++)
                {
                    if (hits[i].collider.gameObject.TryGetComponent(out HurtBox hurtBox))
                    {
                        Debug.Log("BeamHit 1: " + hurtBox.Owner.transform.gameObject.name);
                        hitsHurtBoxes.Add(hits[i]);
                        dicHurtBoxByHit.Add(hits[i], hurtBox);
                    }
                }
            }

            lineRenderer.SetPosition(0, owner.SkillOrigin.position);

            hitsHurtBoxes = hitsHurtBoxes.OrderBy(x => Vector3.Distance(owner.SkillOrigin.position, x.point)).ToList();

            numHits = hitsHurtBoxes.Count <= maxTargets ? hitsHurtBoxes.Count : maxTargets;
            
            int numberOfHits = 0;
            if ( timer > hitTime)
            {
                if (!hasHitTimeReached) hasHitTimeReached = true;

                for (int i = 0; i < numHits; i++)
                {
                    // Handle Max Targets hit
                    if (numberOfHits >= beamSkill.MaxTargets) break;

                    // find hurtBox
                    HurtBox hurtBox = dicHurtBoxByHit[hitsHurtBoxes[i]];

                    // Handle interact with itself
                    if (hurtBox.Owner.transform == owner.transform && !beamSkill.CanSelfHit) continue;

                    // Check if already hit target
                    if (targets.Contains(hurtBox.Owner)) continue;

                    if (Hit(hurtBox.Owner, hitsHurtBoxes[i].point))
                    {
                        // Pegar informação do dano e mantar para o target
                        hurtBox.GotHit(this);

                        targets.Add(hurtBox.Owner);
                        numberOfHits++;
                    }

                }
                timer = 0;
                targets.Clear();
            }

            if (hitsHurtBoxes.Count == 0)
            {
                // Nothing hit, use beam MaxDistance
                lineRenderer.SetPosition(1, owner.SkillOrigin.position + beamSkill.MaxDistance * owner.transform.right);
                return;
            }

            lineRenderer.SetPosition(1, hitsHurtBoxes[numHits - 1].point);
        }
        public bool Hit(IHittable target, Vector3 hitPosition)
        {
            if (!HasAuthority) return false;

            // TODO: create logic to not hit itself

            target.GetOwnership();

            HitInfo = new HitInfo(beamSkill.GetSkillDamage(owner.ValueByStat), beamSkill.DamageClass, beamSkill.GetSkillImpact(owner.ValueByStat), hitPosition, beamSkill.Buffs, beamSkill.HitType);
            return true;
        }
        public override void GetFromPool()
        {

        }
        public override void ReleaseToPool()
        {
            gameObject.SetActive(false);
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }
        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
