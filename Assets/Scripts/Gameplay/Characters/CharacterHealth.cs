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
        [SerializeField] protected NetworkVariable<int> health = new NetworkVariable<int>(100,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        [SerializeField] protected int constitution;
        // [Tooltip("Maximum health points of the character.")]
        [SerializeField] protected int maxHealth;
        public int CurrentHealth { get { return health.Value; }}
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
        [SerializeField] protected NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool IsAlive { get { return isAlive.Value;}}
        public bool IsDead { get { return !isAlive.Value;}}
        // [SerializeField] protected int _maxWounds = 4;
        [field: SerializeField] public int MaxWounds { get; private set; }
        [SerializeField] protected int woundHealth = 25;
        [SerializeField] protected int wounds;

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

        public void SetHealthParameters(int constitution, List<CharacterTrait> traits)
        {
            this.constitution = constitution;
            OriginalMaxHealth = woundHealth * constitution;

            if (maxHealth > OriginalMaxHealth)
            {
                maxHealth = OriginalMaxHealth;
            }

            MaxWounds = constitution;

            if (wounds > MaxWounds)
            {
                 wounds = MaxWounds;
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
            maxHealth = OriginalMaxHealth;
            wounds = MaxWounds;
            if (HasAuthority)
            {
                health.Value = maxHealth;
            }
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

            
            
            // if (healthValue < (wounds - 1) * woundHealth)
            // {
            //     wounds--;
            //     maxHealth -= woundHealth;
            // }

            // Don't let health be negative
            if (healthValue < 0) healthValue = 0;

            wounds = Mathf.CeilToInt((float) healthValue / woundHealth);
            maxHealth = woundHealth * wounds;

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

            if (health.Value == maxHealth) return;

            if (!HasAuthority) return;

            int healthValue = health.Value;

            healthValue += healAmount;

            if (healthValue > maxHealth)
            {
                healthValue = maxHealth;
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

