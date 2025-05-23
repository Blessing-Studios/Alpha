using Blessing.Audio;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blessing.VFX;

namespace Blessing.HealthAndDamage
{
    [Serializable] public struct HurtEffect
    {
        public HitType HitType;
        public PooledEffect HitEffect;
        public AudioClip[] AudioClips;
    }
    public class HurtBox : MonoBehaviour
    {
        [Tooltip("Owner of this HurtBox")][field: SerializeField] public GameObject OwnerGameObject { get; private set; }
        public IHittable Owner { get; private set; }
        [Tooltip("Damage multiplier when hitting this hurbox")][field: SerializeField] public float DmgMultiplier { get; private set; }
        [field: SerializeField] protected HitType defaultHitType = HitType.Slash;
        [field: SerializeField] protected List<HurtEffect> hurtEffects = new();

        void Awake()
        {
            // Check if the fields are filled
            if (OwnerGameObject == null)
                Debug.LogWarning("Owner Field is required: " + gameObject.transform.parent.name);

            if (DmgMultiplier < 0)
                Debug.LogWarning("Damage multiplier Field can't be less than zero: " + gameObject.transform.parent.name);
                
            Owner = OwnerGameObject.GetComponent<IHittable>();
            if (Owner == null)
                Debug.LogError("Owner must has IHittable interface: " + gameObject.transform.parent.name);
        }
        public void GotHit(IHitter hitter)
        {
            Owner.GotHit(hitter, this);
        }
        public void TriggerHitEffect(HitInfo hitInfo)
        {
            Debug.Log(gameObject.name + ": Entrou TriggerHitEffect");
            foreach (HurtEffect hurtEffect in hurtEffects)
            {
                if (hurtEffect.HitType == hitInfo.HitType)
                {
                    if (hurtEffect.HitEffect != null)
                        PoolManager.Singleton.Get<PooledEffect>(hurtEffect.HitEffect).Initialized(hitInfo.HitPosition);
                    if (hurtEffect.AudioClips.Length > 0)
                    {
                        Debug.Log(gameObject.name + ": hurtEffect.AudioClips.Length > 0");
                        AudioManager.Singleton.PlaySoundFx(hurtEffect.AudioClips, transform);
                    }
                        


                    return;
                }
            }

            // If Can't find a hurtEffect if proper HitType, uses default HitType
            foreach(HurtEffect hurtEffect in hurtEffects)
            {
                if (hurtEffect.HitType == defaultHitType)
                {
                    if (hurtEffect.HitEffect != null)
                        PoolManager.Singleton.Get<PooledEffect>(hurtEffect.HitEffect).Initialized(hitInfo.HitPosition);
                    if (hurtEffect.AudioClips.Length > 0)
                        AudioManager.Singleton.PlaySoundFx(hurtEffect.AudioClips, transform);

                    return;
                }
            }
        }
    }
}