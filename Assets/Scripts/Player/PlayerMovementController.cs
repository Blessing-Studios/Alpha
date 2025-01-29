using System.Collections;
using System.Collections.Generic;
using Blessing.GameData;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blessing.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovementController : MovementController
    {
        [SerializeField] private bool canGiveInputs = false;
        [field: SerializeField] public bool IsOffline {get; private set;}
        protected PlayerInput playerInput;
        protected InputAction moveAction;
        protected InputActionMap characterControlsMap;
        protected PlayerCharacter playerCharacter;
        

        // Awake is called earlier than Start
        protected override void Awake()
        {
            base.Awake();
            playerInput = GetComponent<PlayerInput>();
            playerCharacter = GetComponent<PlayerCharacter>();
        }

        void Start()
        {
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(playerCharacter.GetPlayerOwnerName());
        }

        // Update is called once per frame
        protected override void Update()
        {
            
            if (HasAuthority || IsOffline)
            {
                base.Update();
            }
        }

        public override void EnableMovement()
        {
            base.EnableMovement();
            HandlePlayerMovement(currentMovementInput);
        }

        /// <summary>
        /// OnMove is created to be used with the Component Plater Input
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!HasAuthority || !canGiveInputs) return;

            if (context.performed || context.canceled)
            {
                currentMovementInput = context.ReadValue<Vector2>();
                HandlePlayerMovement(currentMovementInput);
            }
            
        }
        protected void HandlePlayerMovement(Vector2 currentMovementInput)
        {
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

            if (isMovementPressed)
            {
                animator.SetBool(isWalkingHash, true);
            }
            else
            {
                animator.SetBool(isWalkingHash, false);
            }
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            canGiveInputs = GameDataManager.Singleton.ValidateOwner(playerCharacter.GetPlayerOwnerName());
        }
    }
}


