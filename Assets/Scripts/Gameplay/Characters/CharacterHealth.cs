using Blessing.Gameplay.Characters;
using Blessing.HealthAndDamage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Core.GameEventSystem;
using System;

namespace Blessing.Gameplay.Characters
{
    public class CharacterHealth : NetworkBehaviour, IHealth
    {
        // Tentar criar a classe sem usar uma MonoBehaviour
        [field: SerializeField] public bool ShowDebug { get; private set; }

        [Header("Health Settings")]
        [Tooltip("Current health points of the character.")]
        // [SerializeField] protected int health;

        public int Health { get { return health.Value; } protected set { health.Value = value; } }
        [SerializeField]
        protected NetworkVariable<int> health = new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [SerializeField] protected int constitution;
        // [Tooltip("Maximum health points of the character.")]
        public int MaxHealth { get { return maxHealth.Value; } protected set { maxHealth.Value = value; } }
        [SerializeField]
        protected NetworkVariable<int> maxHealth = new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public int CurrentHealth { get { return health.Value; } }
        [SerializeField] public int OriginalMaxHealth { get; private set; }

        [Tooltip("Base Life Regeneration by time")] // TODO: Fazer life regen depender da mana verde
        [SerializeField] protected int baseRegen = 1;
        [Tooltip("Base Damage over Time")]
        [SerializeField] protected int baseDecay = 0;
        [Tooltip("Life Regeneration by time")][SerializeField] protected int regen;
        [Tooltip("Damage over Time")]
        [SerializeField] protected int decay;
        [Tooltip("Time to wait for health change ove time")]
        [SerializeField] protected float waitTime = 0.5f;
        [field: SerializeField] public List<HurtBox> HurtBoxes { get; protected set; }
        [SerializeField]
        protected NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool IsAlive { get { return isAlive.Value; } }
        public bool IsDead { get { return !isAlive.Value; } }
        // [SerializeField] protected int _maxWounds = 4;
        [field: SerializeField] public int MaxWounds { get; protected set; }
        [SerializeField] protected int woundHealth = 25;
        [SerializeField] protected int Wounds { get { return wounds.Value; } set { wounds.Value = value; } }
        [SerializeField]
        protected NetworkVariable<int> wounds = new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        [Header("Events")]
        public GameEvent OnReceiveDamage;
        public GameEvent OnHealthChanged;
        public GameEvent OnIsAliveChanged;
        private bool isHealthInitialized = false;
        // Wounds are the subDivision of the HeathPool, they are like block of health
        // If a character loose health equal to woundHealth, the character will loose Max health equal to woundHealth
        // it is like the Character is wounded and will not full recovery

        // Start is called before the first frame update
        void Awake()
        {

        }
        void Start()
        {

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                ReceiveHeal(10);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ReceiveDamage(10);
            }
        }

        public override void OnNetworkSpawn()
        {
            health.OnValueChanged += OnNetworkHealthChanged;
            // StartCoroutine(ChangeLifeByTime());

            isAlive.OnValueChanged += OnNetworkIsAliveChanged;
        }



        public override void OnNetworkDespawn()
        {
            health.OnValueChanged -= OnNetworkHealthChanged;
            isAlive.OnValueChanged -= OnNetworkIsAliveChanged;

            StopAllCoroutines();
        }
        public void OnNetworkHealthChanged(int previousValue, int newValue)
        {
            if (OnHealthChanged != null)
                OnHealthChanged.Raise(this);
        }
        private void OnNetworkIsAliveChanged(bool previousValue, bool newValue)
        {
            if (OnIsAliveChanged != null)
                OnIsAliveChanged.Raise(this, newValue);
        }

        public void ApplyPermanentEffects(PermanentEffect[] permanentEffects)
        {
            Debug.Log("ApplyPermanentEffects");
            foreach (PermanentEffect permEffect in permanentEffects)
            {
                foreach (HealthPermanentChange healthChange in permEffect.HealthChanges)
                {
                    if (healthChange.Health < 0) ReceiveDamage(-1 * healthChange.Health);

                    ChangeWound(healthChange.Wound);

                    if (healthChange.Health > 0) ReceiveHeal(healthChange.Health);
                }
            }
        }

        public void SetHealthParameters(int constitution, List<CharacterTrait> traits)
        {
            if (!HasAuthority) return;
            
            this.constitution = constitution;
            OriginalMaxHealth = woundHealth * constitution;
            MaxWounds = constitution;


            if (MaxHealth > OriginalMaxHealth)
            {
                MaxHealth = OriginalMaxHealth;
            }

            if (Wounds > MaxWounds)
            {
                Wounds = MaxWounds;
            }

            if (!isHealthInitialized)
            {
                Initialize();
                isHealthInitialized = true;
            }

            regen = baseRegen;
            decay = baseDecay;

            // Apply Traits
            foreach (CharacterTrait characterTrait in traits)
            {
                regen += characterTrait.Trait.GetHealthRegenChange();
                decay += characterTrait.Trait.GetHealthDecayChange();
            }
        }
        public void Initialize()
        {

            if (HasAuthority)
            {
                MaxHealth = OriginalMaxHealth;
                Wounds = MaxWounds;
                health.Value = MaxHealth;
            }
        }
        public int GetHealth()
        {
            return health.Value;
        }

        public int GetWounds()
        {
            return Wounds;
        }

        protected void ChangeWound(int value)
        {
            if (!HasAuthority) return;
            if (value == 0) return;

            Debug.Log("ChangeWound: " + value);

            int newValue = Wounds + value;

            if (newValue < 1)
            {
                Wounds = 1;
                return;
            }

            if (newValue > MaxWounds)
            {
                Wounds = MaxWounds;
                return;
            }

            Wounds = newValue;
            MaxHealth = woundHealth * Wounds;
        }

        public int GetWoundHealth()
        {
            return woundHealth;
        }

        public float GetHealthPercent()
        {
            return (float)health.Value / OriginalMaxHealth;
        }

        public void SetCharacterAsAlive()
        {
            if (HasAuthority)
                isAlive.Value = true;
        }

        public void SetCharacterAsDead()
        {
            if (HasAuthority)
            {
                health.Value = 0;
                isAlive.Value = false;
            }
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
            // TODO



            // if (healthValue < (Wounds - 1) * woundHealth)
            // {
            //     Wounds--;
            //     MaxHealth -= woundHealth;
            // }

            // Don't let health be negative
            if (healthValue < 0) healthValue = 0;

            // Set new value for Wounds and MaxHealth
            Wounds = Mathf.CeilToInt((float)healthValue / woundHealth);
            MaxHealth = woundHealth * Wounds;

            health.Value = healthValue;
            if (ShowDebug) Debug.Log(gameObject.name + " damageAmount: " + damageAmount);
            if (ShowDebug) Debug.Log(gameObject.name + " healthValue: " + healthValue);

            if (health.Value == 0) SetCharacterAsDead();

            // Raise Events
            if (OnReceiveDamage != null)
                OnReceiveDamage.Raise(this, damageAmount);
        }

        public void ReceiveHeal(int healAmount)
        {
            if (healAmount <= 0) return;

            if (health.Value == MaxHealth) return;

            if (!HasAuthority) return;

            int healthValue = health.Value;

            healthValue += healAmount;

            if (healthValue > MaxHealth)
            {
                healthValue = MaxHealth;
            }

            health.Value = healthValue;
        }
        public void ReceiveBleed(int bleedAmount)
        {
            if (bleedAmount <= 0) return;

            if (!HasAuthority) return;

            int healthValue = health.Value;

            healthValue -= bleedAmount;

            // Don't let health be negative
            if (healthValue < 0) healthValue = 0;

            health.Value = healthValue;

            if (health.Value == 0) SetCharacterAsDead();
        }
        public void ChangeByTime()
        {
            ReceiveHeal(regen);
            ReceiveBleed(decay);
        }
    }
}

