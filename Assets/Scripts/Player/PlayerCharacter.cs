using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.InputActions;
using InputActionType = Blessing.Gameplay.Characters.InputActions.InputActionType;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.GameData;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.InputSystem;
using Blessing.Gameplay.HealthAndDamage;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using NUnit.Framework;

namespace Blessing.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Interactor))]
    public class PlayerCharacter : Character
    {
        // [HideInInspector] public NetworkVariable<FixedString32Bytes> OwnerName = new(); // Mover para o PlayerCharacterNetwork

        // public string PlayerOwnerName; // Mover para PlayerCharacterNetwork
        // public NetworkVariable<bool> IsDisabled = new(); // Move to PlayerCharacterNetwork
        // private int deferredDespawnTicks = 4;

        [field: SerializeField] public PlayerCharacterNetwork Network { get; private set; }
        public void SetPlayerCharacterNetwork( PlayerCharacterNetwork network)
        {
            Network = network;
        }
        private PlayerInput playerInput;
        public Interactor Interactor { get; private set; }
        private bool isPlayerCharacterInitialized = false;
        [SerializeField] private bool canGiveInputs = false;
        public bool CanGiveInputs { get { return  canGiveInputs; } }
        public void SetCanGiveInputs (bool canGiveInputs)
        {
            this.canGiveInputs = canGiveInputs;
        }
        
        public string GetPlayerOwnerName()
        {
            if (Network != null)
                return Network.OwnerName.Value.ToString();

            return "Player1";
        }

        public void SetPlayerOwnerName(string name)
        {
            if (Network == null) return;

            Network.SetPlayerOwnerName(name);
            if (ShowDebug) Debug.Log(gameObject.name + ": SetPlayerOwnerName");
        }

        // public void SetOwnerName(string name) // Mover para PlayerCharacterNetwork
        // {
        //     OwnerName.Value = new FixedString32Bytes(name);
        // }

        // public override void OnNetworkSpawn() // Remover para testar o CharacterNetwork
        // {
        //     base.OnNetworkSpawn();
        // }
        protected override void Awake()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Awake");
            base.Awake();

            playerInput = GetComponent<PlayerInput>();

            Interactor = GetComponent<Interactor>();

            Network = GetComponent<PlayerCharacterNetwork>();

            GameManager.Singleton.PlayerCharacterList.Add(this);
        }

        protected override void Start()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Start");
            base.Start();
        }

        public override void Initialize()
        {
            if (isPlayerCharacterInitialized) return;

            isPlayerCharacterInitialized = true;

            base.Initialize();

            gameObject.name = "Char-" + GetPlayerOwnerName();
            GameManager.Singleton.AddPlayerCharacter(GetPlayerOwnerName(), this);
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(GetPlayerOwnerName());
            if (ShowDebug) Debug.Log(gameObject.name + ": Initialize");
        }

        public void Update()
        {
            // PlayerOwnerName = OwnerName.Value.ToString(); // Mover para PlayerCharacterNetwork
        }


        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!HasAuthority || !canGiveInputs) return;

            if (context.performed)
            {
                TriggerAction = GetAction("Attack");
                CharacterStateMachine.CharacterState.OnTrigger(TriggerAction, TriggerDirection);
            }
        }
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!HasAuthority || !canGiveInputs) return;
            
            Vector2 currentMovementInput = Vector2.zero;
            if (context.performed || context.canceled)
            {
                currentMovementInput = context.ReadValue<Vector2>();
            }

            TriggerDirection = currentMovementInput switch
            {
                Vector2 v when v.x > 0.6f => GetDirection("Forward"),
                Vector2 v when v.x < -0.6f => GetDirection("Forward"),
                Vector2 v when v.y > 0.6f => GetDirection("Up"),
                Vector2 v when v.y < -0.6f => GetDirection("Down"),
                _ => GetDirection("Any"),
            };
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!HasAuthority || !canGiveInputs) return;

            if (context.performed)
            {
                Interactor.HandleInteraction();
            }
        }

        public override bool CheckIfActionTriggered(string actionName)
        {
            if (!HasAuthority || !canGiveInputs) return false;

            if (actionName != ""
                && playerInput.currentActionMap.FindAction(actionName) != null
                && playerInput.currentActionMap.FindAction(actionName).triggered)
            {
                return true;
            }

            return false;
        }

        public override bool Hit(IHittable target)
        {
            bool baseValue = base.Hit(target);

            if (HasAuthority && baseValue) target.GetOwnership();

            return baseValue;
        }
    }
}