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

namespace Blessing.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCharacter : Character
    {
        [HideInInspector] public NetworkVariable<FixedString32Bytes> OwnerName = new();
        public NetworkVariable<bool> IsDisabled = new();
        // private int deferredDespawnTicks = 4;

        [field: SerializeField] private InputActionList actionList;
        [field: SerializeField] private InputDirectionList directionList;
        public InputActionType TriggerAction { get; private set; }
        public InputDirectionType TriggerDirection { get; private set; }
        private PlayerInput playerInput;
        private Dictionary<string, InputActionType> inputActionsDic = new();
        private Dictionary<string, InputDirectionType> inputDirectionsDic = new();
        private bool canGiveInputs = false;
        public string GetOwnerName()
        {
            return OwnerName.Value.ToString();
        }

        protected override void Awake()
        {
            base.Awake();

            playerInput = GetComponent<PlayerInput>();

            foreach (InputActionType InputAction in actionList.InputActions)
            {
                inputActionsDic.Add(InputAction.Name, InputAction);
            }

            foreach (InputDirectionType InputDirection in directionList.InputDirections)
            {
                inputDirectionsDic.Add(InputDirection.Name, InputDirection);
            }
        }

        void Start()
        {
            HandleInitialization();
        }

        public void Update()
        {
            if (IsDisabled.Value == true && gameObject.activeSelf == true)
            {
                gameObject.SetActive(false);
                // NetworkObject.DeferDespawn(deferredDespawnTicks, destroy: true);
            }
        }

        private void HandleInitialization()
        {
            if (HasAuthority)
            {
                IsDisabled.Value = false;
                OwnerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);

                if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
                {
                    PlayerCharacter originalCharacter = GameManager.Singleton.PlayerCharactersDic[GetOwnerName()];

                    originalCharacter.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

                    GameManager.Singleton.VirtualCamera.LookAt = originalCharacter.transform;
                    GameManager.Singleton.VirtualCamera.Target.TrackingTarget = originalCharacter.transform;

                    IsDisabled.Value = true;
                }
                else // This player doesn't have a Char in this session, use this char
                {
                    GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
                    GameManager.Singleton.PlayerCharacterList.Add(this);

                    GameManager.Singleton.VirtualCamera.LookAt = transform;
                    GameManager.Singleton.VirtualCamera.Target.TrackingTarget = transform;
                }
            }

            if (!HasAuthority && !GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
            {
                GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
                GameManager.Singleton.PlayerCharacterList.Add(this);
            }

            gameObject.name = gameObject.name + "-" + GetOwnerName();

            canGiveInputs = GameDataManager.Singleton.PlayerName == GetOwnerName();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            if (!HasAuthority) return;

            if (GameDataManager.Singleton.PlayerName == GetOwnerName())
                GetComponent<NetworkObject>().SetOwnershipLock(true);
            else
            {
                GetComponent<NetworkObject>().SetOwnershipLock(false);
            }

            canGiveInputs = GameDataManager.Singleton.PlayerName == GetOwnerName();
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
    }
}