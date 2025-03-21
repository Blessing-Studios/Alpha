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
using Unity.Cinemachine;
using Unity.VisualScripting;

namespace Blessing.Player
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
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
        private CinemachineImpulseSource impulseSource;
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

            impulseSource = GetComponent<CinemachineImpulseSource>();

            GameManager.Singleton.PlayerCharacterList.Add(this);
        }

        protected override void Start()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Start");
            base.Start();
        }

        public void InitializePlayerChar()
        {
            if (isPlayerCharacterInitialized) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": InitializePlayerChar");

            

            gameObject.name = "Char-" + GetPlayerOwnerName();
            GameManager.Singleton.AddPlayerCharacter(GetPlayerOwnerName(), this);
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(GetPlayerOwnerName());

            foreach(PlayerController player in GameManager.Singleton.Players)
            {
                if (player.GetPlayerName() == GetPlayerOwnerName() && player.PlayerCharacter == null)
                {
                    player.SetPlayerCharacter(this);
                }
            }

            isPlayerCharacterInitialized = true;
        }

        public void GetPlayerCharacterOwnership()
        {
            base.GetOwnership();
        }
        public override void GetOwnership()
        {
            // TODO: mudar lógica para não passar Ownership
        }
        public void Update()
        {
            // PlayerOwnerName = OwnerName.Value.ToString(); // Mover para PlayerCharacterNetwork
        }


        public void OnAttack(InputAction.CallbackContext context)
        {
            // if (!HasAuthority || !canGiveInputs) return;

            // if (context.performed)
            // {
            //     TriggerAction = GetAction("Attack");
            //     CharacterStateMachine.CharacterState.OnTrigger(TriggerAction, TriggerDirection);
            // }

            HandleActionInput(context);
        }

        public void OnSpecial(InputAction.CallbackContext context)
        {
            
            // if (!HasAuthority || !canGiveInputs) return;

            // // Checar se tem alguma Spell em quick spell slot

            // // Caso não tenha um spell em quick spell, tratar special como trigger de combo
            // if (context.performed)
            // {
            //     TriggerAction = GetAction("Special");
            //     CharacterStateMachine.CharacterState.OnTrigger(TriggerAction, TriggerDirection);
            // }

            HandleActionInput(context);
        }

        public void OnHoldSpecial(InputAction.CallbackContext context)
        {
            // Precisa arrumar o Hold

            // if (!HasAuthority || !canGiveInputs) return;

            // // Checar se tem alguma Spell em quick spell slot

            // // Caso não tenha um spell em quick spell, tratar special como trigger de combo
            // if (context.performed)
            // {
            //     TriggerAction = GetAction("HoldSpecial");
            //     CharacterStateMachine.CharacterState.OnTrigger(TriggerAction, TriggerDirection);
            // }

            HandleActionInput(context);
        }

        private void HandleActionInput(InputAction.CallbackContext context)
        {
            if (!HasAuthority || !canGiveInputs) return;

            // Checar se tem alguma Spell em quick spell slot

            // Caso não tenha um spell em quick spell, tratar special como trigger de combo
            if (context.performed)
            {
                TriggerAction = GetAction(context.action.name);
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

            if (HasAuthority && baseValue)
            {
                if (CharacterStateMachine.CurrentMove.ShakeEffect != null)
                    GameManager.Singleton.CameraShake(impulseSource, CharacterStateMachine.CurrentMove.ShakeEffect);
                    
                target.GetOwnership();
            }

            return baseValue;
        }

        public override void GotHit(IHitter hitter)
        {
            base.GotHit(hitter);
        }
    }
}