using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.HealthAndDamage
{
    public class DamageNumberHandler : MonoBehaviour
    {

        [field: SerializeField] public bool ShowDebug { get; private set; }
        [field: SerializeField] private CharacterHealth owner;
        public PooledObject DamageNumberPrefab;

        void Start()
        {
            if (owner == null)
            {
                Debug.LogError(gameObject.name + ": Owner is missing");
            }

            if (DamageNumberPrefab == null)
            {
                Debug.LogError(gameObject.name + ": DamageNumberPrefab is missing");
            }
        }

        public void OnReceiveDamage(Component component, object data)
        {
            if (component.gameObject != owner.gameObject) return;

            int damage = (int) data;

            // GameManager.Singleton.GetDamageNumber(transform.position, damage);
            PoolManager.Singleton.Get<DamageNumber>(DamageNumberPrefab).Initialize(transform.position, damage);
        }
    }
}

