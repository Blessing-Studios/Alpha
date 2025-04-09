using Blessing.Gameplay.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.HealthAndDamage
{
    public class HurtBox : MonoBehaviour
    {
        [Tooltip("Owner of this HurtBox")][field: SerializeField] public GameObject OwnerGameObject { get; private set; }
        public IHittable Owner { get; private set; }
        public bool IsActive = true;
        [Tooltip("Damage multiplier when hitting this hurbox")][field: SerializeField] public float DmgMultiplier { get; private set; }
        public ParticleSystem TakeHitEffect;

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
    }
}