using Blessing.Gameplay.Characters;
using Blessing.Gameplay.HealthAndDamage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using NUnit.Framework;

namespace Blessing.Gameplay.Characters
{
    public class CharacterHealth : NetworkBehaviour, IHealth
    {
        // Tentar criar a classe sem usar uma MonoBehaviour
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public event EventHandler OnHealthChanged;
        public event EventHandler OnTakeDamage;
        protected IEnumerator coroutine;

        [Header("Health Settings")]
        [Tooltip("Current health points of the character.")]
        // [SerializeField] protected int health;
        [SerializeField]
        protected NetworkVariable<int> health = new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // [Tooltip("Maximum health points of the character.")]
        [SerializeField] protected int maxHealth;
        public int CurrentHealth { get { return health.Value; }}
        [SerializeField] public int OriginalMaxHealth { get; private set; }

        [Tooltip("Life Regeneration by time")]
        [SerializeField] protected int lifeRegen = 1;
        [Tooltip("Damage over Time")]
        [SerializeField] protected int damageOverTime = 0;
        [Tooltip("Time to wait for health change ove time")]
        [SerializeField] protected float waitTime = 0.5f;
        protected bool isAlive = true;
        public bool IsAlive { get { return isAlive;}}
        // [SerializeField] protected int _maxWounds = 4;
        [field: SerializeField] public int MaxWounds { get; private set; }
        [SerializeField] protected int woundHealth = 25;
        [SerializeField] protected int wounds;
        protected Character character;
        // Wounds are the subDivision of the HeathPool, they are like block of health
        // If a character loose health equal to woundHealth, the character will loose Max health equal to woundHealth
        // it is like the Character is wounded and will not full recovery

        // Start is called before the first frame update
        void Awake()
        {
            // character = GetComponent<Character>();
            OnTakeDamage += HandleOnTakeDamage;

            //MaxWounds = (int)(character.Constitution / 2);
            //woundHealth = 5 * character.Constitution;

            MaxWounds = 3;
            woundHealth = 25;

            maxHealth = woundHealth * MaxWounds;
            OriginalMaxHealth = maxHealth;
            wounds = MaxWounds;
        }

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnNetworkHealthChanged;

            if (HasAuthority)
            {
                health.Value = maxHealth;
            }

            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }
        void Start()
        {
            coroutine = ChangeLifeByTime();
            StartCoroutine(coroutine);
        }

        public void OnNetworkHealthChanged(int previous, int current)
        {
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }

        public int GetHealth()
        {
            return health.Value;
        }

        public int GetWounds()
        {
            return wounds;
        }

        public int GetWoundHealth()
        {
            return woundHealth;
        }

        public float GethealthPercent()
        {
            return (float)health.Value / OriginalMaxHealth;
        }

        public bool GetIfCharacterIsAlive()
        {
            return isAlive;
        }

        public void SetCharacterAsAlive()
        {
            StartCoroutine(coroutine);
            isAlive = true;
        }

        public void SetCharacterAsDead()
        {
            StopCoroutine(coroutine);
            health.Value = 0;
            isAlive = false;
        }

        public void ReceiveDamage(int damageAmount)
        {
            // Do nothhing if damage is less or equal to zero
            if (damageAmount <= 0) return;

            if (health.Value == 0) return;

            if (!HasAuthority) return;

            int healthValue = health.Value;

            healthValue -= damageAmount;

            // arrumar essa parte para considerar o netcode
            if (healthValue <= (wounds - 1) * woundHealth)
            {
                wounds--;
                maxHealth -= woundHealth;
            }

            // Don't let health be negative
            if (healthValue < 0) healthValue = 0;

            health.Value = healthValue;
            
            if (ShowDebug) Debug.Log(gameObject.name + " healthValue: " + healthValue);

            OnTakeDamage?.Invoke(this, EventArgs.Empty);
        }

        public void HandleOnTakeDamage(object sender, System.EventArgs eventArgs)
        {
            // Debug.Log("HandleOnTakeDamage: " + character.name);
            // character.GetMeleeStateMachine().CurrentState.OnTakeDamage(sender, eventArgs);
        }

        public void ReceiveHeal(int healAmount)
        {
            if (healAmount <= 0) return;

            if (health.Value == maxHealth) return;

            if (!HasAuthority) return;

            if (health.Value + healAmount > maxHealth)
            {
                health.Value = maxHealth;
                return;
            }

            health.Value += healAmount;
        }
        public void ReceiveBleed(int bleedAmout)
        {
            if (bleedAmout <= 0) return;

            if (!HasAuthority) return;

            int healthValue = health.Value;

            healthValue -= bleedAmout;

            // Don't let health be negative
            if (healthValue < 0) healthValue = 0;

            health.Value = healthValue;
        }
        public IEnumerator ChangeLifeByTime()
        {
            while (isAlive && HasAuthority)
            {
                //Put your code before waiting here

                ReceiveHeal(lifeRegen);
                ReceiveBleed(damageOverTime);

                yield return new WaitForSeconds(waitTime);

                //Put code after waiting here

                //You can put more yield return new WaitForSeconds(1); in one coroutine
            }
        }
    }
}

