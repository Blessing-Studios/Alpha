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
using Blessing.HealthAndDamage;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Blessing.Gameplay.Guild;

namespace Blessing.Player
{
    [RequireComponent(typeof(Adventurer))]
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
        public PlayerInput PlayerInput { get { return playerInput; } }
        public Interactor Interactor { get; private set; }
        private bool isPlayerCharacterInitialized = false;
        private CinemachineImpulseSource impulseSource;
        public Adventurer Adventurer;
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

            Adventurer = GetComponent<Adventurer>();

            GameManager.Singleton.PlayerCharacterList.Add(this);
        }

        protected override void Start()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Start");
            base.Start();
        }
        private bool playerControllersReady = false;
        public void TriggerInitializePlayerChar()
        {
            playerControllersReady = true;

            InitializePlayerChar();
        }
        public void InitializePlayerChar()
        {
            if (isPlayerCharacterInitialized) return;

            if (!Network.HasSpawned) return;
            if (!playerControllersReady) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": InitializePlayerChar");

            gameObject.name = "Char-" + GetPlayerOwnerName();
            GameManager.Singleton.AddPlayerCharacter(GetPlayerOwnerName(), this);
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(GetPlayerOwnerName());
            
            if (canGiveInputs) 
                playerInput.enabled = true;
            else
                playerInput.enabled = false;

            foreach(PlayerController player in GameManager.Singleton.Players)
            {
                if (player.GetPlayerName() == GetPlayerOwnerName() && player.PlayerCharacter == null)
                {
                    player.SetPlayerCharacter(this);
                }
            }
        }
        public override bool CheckIfActionTriggered(InputActionType actionType)
        {
            if (!HasAuthority || !canGiveInputs) return false;

            string actionName = actionType.Name;

            if (actionName != ""
                && playerInput.currentActionMap.FindAction(actionName) != null
                && playerInput.currentActionMap.FindAction(actionName).triggered)
            {
                return true;
            }

            return false;
        }

        public override bool CheckIfDirectionTriggered(InputDirectionType directionType)
        {
            if (directionType == null || directionType == GetDirection("Any")) return true;

            return directionType == TriggerDirection;
        }

        public override bool CheckIfComboMoveTriggered(Move move)
        {
            return CheckIfActionTriggered(move.TriggerAction) && CheckIfDirectionTriggered(move.TriggerDirection);
        }

        public override bool Hit(IHittable target, Vector3 hitPosition)
        {
            bool baseValue = base.Hit(target, hitPosition);

            if (HasAuthority && baseValue)
            {       
                target.GetOwnership();
            }

            return baseValue;
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

        public void OnAbility1(InputAction.CallbackContext context)
        {
            if (Abilities.Count > 0)
                HandleActionInput(context);
        }

        public void OnAbility2(InputAction.CallbackContext context)
        {
            if (Abilities.Count > 1)
                HandleActionInput(context);
        }

        public void OnAbility3(InputAction.CallbackContext context)
        {
            if (Abilities.Count > 2)
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

            if (context.canceled)
            {
                TriggerAction = null;
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

        public override void OnDeath()
        {
            base.OnDeath();
        }
        public override void OnAnimationAttack()
        {
            base.OnAnimationAttack();

            if (HasAuthority && CharacterStateMachine.CurrentMove.ShakeEffect != null)
                    GameManager.Singleton.CameraShake(impulseSource, CharacterStateMachine.CurrentMove.ShakeEffect);

        }
    }
}