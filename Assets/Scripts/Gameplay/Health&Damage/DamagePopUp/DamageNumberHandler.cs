using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Gameplay.HealthAndDamage
{
    public class DamageNumberHandler : MonoBehaviour
    {

        [field: SerializeField] public bool ShowDebug { get; private set; }
        [field: SerializeField] private CharacterHealth owner;

        void Start()
        {
            if (owner == null)
            {
                Debug.LogError(gameObject.name + ": Owner is missing");
            }
        }

        public void OnReceiveDamage(Component component, object data)
        {
            if (component.gameObject != owner.gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": DamageNumberHandler OnReceiveDamage");

            int damage = (int) data;

            GameManager.Singleton.GetDamageNumber(transform.position, damage);
        }
    }
}

