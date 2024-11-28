using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class MoveState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;

        public Move CurrentMove;
        private int moveIndex;
        private int comboIndex;
        protected float attackPressedTimer = 0;
        public MoveState(CharacterStateMachine _characterStateMachine) : base(_characterStateMachine)
        {
            movementController = character.GetMovementController();
            characterController = movementController.GetCharacterController();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            moveIndex = characterStateMachine.MoveIndex;
            comboIndex = characterStateMachine.ComboIndex;
            CurrentMove = characterStateMachine.GetAllCombos()[comboIndex].Moves[moveIndex];


            character.GetMovementController().DisableMovement();

            // Trigger animation
            animator.SetTrigger(CurrentMove.AnimationParam);

            // Zera movementController.AttackMovement
            movementController.AttackMovement = Vector3.zero;

            duration = CurrentMove.AnimationClip.length;
        }

        // public override bool OnTrigger(InputActionsList triggerActionName, InputDirectionsList triggerDirection)
        // {
        //     characterStateMachine.CurrentAction = triggerActionName;
        //     characterStateMachine.CurrentDirectionAction = triggerDirection;
        //     characterStateMachine.SetNextState(characterStateMachine.CombatEntryState);

        //     return true;
        // }

        public override void OnUpdate()
        {
            base.OnUpdate();

            shouldCombo = false;
            attackPressedTimer -= Time.deltaTime;

            // Precias setar na animação a variável AttackMovement
            movementController.HandleAttackMovment();

            string nextComboAction = "";
            if (moveIndex + 1 < characterStateMachine.GetAllCombos()[comboIndex].Moves.Length)
            {
                nextComboAction = characterStateMachine.GetAllCombos()[comboIndex].Moves[moveIndex + 1].TriggerAction.ToString();;
            }

            //  Check if the nextComboAction was pressed
            if (character.CheckIfAttackPressed(nextComboAction))
            {
                attackPressedTimer = character.AttackPressedTimerWindow;
            }

            // Check if the button was pressed in the attack window
            if (animator.GetFloat("AttackWindowOpen") > 0.5f && attackPressedTimer >= 0)
            {
                shouldCombo = true;
            }
            
            // If conditions are match, call next move
            if (time >= duration - CurrentMove.ExitEarlier)
            {
                if (shouldCombo && (moveIndex + 1 < characterStateMachine.GetAllCombos()[comboIndex].Moves.Length))
                {
                    characterStateMachine.MoveIndex = moveIndex + 1;
                    characterStateMachine.SetNextState(characterStateMachine.MoveState);
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            shouldCombo = false;
            movementController.EnableMovement();
            attackPressedTimer = 0;
        }
    }
}
