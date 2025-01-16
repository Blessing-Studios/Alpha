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

        [field: SerializeField] private InputActionList actionList;
        [field: SerializeField] private InputDirectionList directionList;
        public InputActionType TriggerAction { get; private set; }
        public InputDirectionType TriggerDirection { get; private set; }
        private PlayerInput playerInput;
        public Interactor Interactor { get; private set; }
        private Dictionary<string, InputActionType> inputActionsDic = new();
        private Dictionary<string, InputDirectionType> inputDirectionsDic = new();
        [SerializeField] private bool canGiveInputs = false;
        public void SetCanGiveInputs (bool canGiveInputs)
        {
            this.canGiveInputs = canGiveInputs;
        }
        
        public string GetOwnerName()
        {
            if (Network != null)
                return Network.OwnerName.Value.ToString();

            return "Player1";
        }

        public void SetOwnerName(string name)
        {
            if (Network == null) return;

            Network.SetOwnerName(name);    
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
            base.Awake();

            playerInput = GetComponent<PlayerInput>();

            Interactor = GetComponent<Interactor>();

            Network = GetComponent<PlayerCharacterNetwork>();

            foreach (InputActionType InputAction in actionList.InputActions)
            {
                inputActionsDic.Add(InputAction.Name, InputAction);
            }

            foreach (InputDirectionType InputDirection in directionList.InputDirections)
            {
                inputDirectionsDic.Add(InputDirection.Name, InputDirection);
            }
        }

        protected override void Start()
        {
            base.Start();
            // HandleInitialization();
            
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(GetOwnerName());

            if (!HasAuthority && !GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
            {
                GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
                GameManager.Singleton.PlayerCharacterList.Add(this);

                gameObject.name = "Char-" + GetOwnerName();

                // Find Player Controller
                // foreach (PlayerController player in GameManager.Singleton.PlayerList)
                // {
                //     if (player.GetPlayerName() == GetOwnerName())
                //     {
                //         player.SetPlayerCharacter(this);
                //         return;
                //     }
                // }
            }
        }

        public void Update()
        {
            // PlayerOwnerName = OwnerName.Value.ToString(); // Mover para PlayerCharacterNetwork
        }

        // private void HandleInitialization()
        // {
        //     if (HasAuthority)
        //     {
        //         IsDisabled.Value = false;
        //         OwnerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);

        //         if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
        //         {
        //             PlayerCharacter originalCharacter = GameManager.Singleton.PlayerCharactersDic[GetOwnerName()];

        //             originalCharacter.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

        //             GameManager.Singleton.VirtualCamera.LookAt = originalCharacter.transform;
        //             GameManager.Singleton.VirtualCamera.Target.TrackingTarget = originalCharacter.transform;

        //             IsDisabled.Value = true;
        //         }
        //         else // This player doesn't have a Char in this session, use this char
        //         {
        //             GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
        //             GameManager.Singleton.PlayerCharacterList.Add(this);

        //             GameManager.Singleton.VirtualCamera.LookAt = transform;
        //             GameManager.Singleton.VirtualCamera.Target.TrackingTarget = transform;
        //         }
        //     }

        //     if (!HasAuthority && !GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
        //     {
        //         GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
        //         GameManager.Singleton.PlayerCharacterList.Add(this);
        //     }

        //     gameObject.name = gameObject.name + "-" + GetOwnerName();

        //     canGiveInputs = GameDataManager.Singleton.PlayerName == GetOwnerName();
        // }

        // protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        // {
        //     if (!HasAuthority) return;

        //     if (GameDataManager.Singleton.PlayerName == GetOwnerName())
        //         GetComponent<NetworkObject>().SetOwnershipLock(true);
        //     else
        //     {
        //         GetComponent<NetworkObject>().SetOwnershipLock(false);
        //     }

        //     canGiveInputs = GameDataManager.Singleton.PlayerName == GetOwnerName();
        // }


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

        public InputActionType GetAction(string name)
        {
            return inputActionsDic[name];
        }

        public InputDirectionType GetDirection(string name)
        {
            return inputDirectionsDic[name];
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

        public override void AddBackpack(InventoryItem inventoryItem)
        {
            base.AddBackpack(inventoryItem);
            
            // If this is the Local Player, change PlayerInventoryGrid
            if (HasAuthority)
            { 
                GameManager.Singleton.InventoryController.PlayerCharacter = this;
                GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = Gear.Inventory;
                Gear.Inventory.InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;
            }
        }

        public override void RemoveBackpack()
        {
            // If this is the Local Player, change PlayerInventoryGrid
            if (Gear.Inventory != null && HasAuthority)
            {
                GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = null;
                Gear.Inventory.InventoryGrid = GameManager.Singleton.InventoryController.OtherInventoryGrid;
            }

            base.RemoveBackpack();
        }
    }
}