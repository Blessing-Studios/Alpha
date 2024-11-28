using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class MoveState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;
        protected Combo[] combos;

        public Move CurrentMove;
        private int moveIndex;
        private int comboIndex;
        protected float attackPressedTimer = 0;
        public MoveState(CharacterStateMachine _characterStateMachine) : base(_characterStateMachine)
        {
            movementController = characterStateMachine.MovementController;
            characterController = characterStateMachine.CharacterController;

        }

        public override void OnEnter()
        {
            base.OnEnter();

            
            combos = characterStateMachine.GetAllCombos();
            moveIndex = characterStateMachine.MoveIndex;
            comboIndex = characterStateMachine.ComboIndex;
            CurrentMove = combos[comboIndex].Moves[moveIndex];

            Debug.Log("OnEnter Move Name: " + CurrentMove.Name);

            movementController.DisableMovement();

            // Trigger animation
            animator.SetTrigger(CurrentMove.AnimationParam);

            // Zera movementController.AttackMovement
            movementController.AttackMovement = Vector3.zero;

            duration = CurrentMove.AnimationClip.length;

            shouldCombo = false;
            attackPressedTimer = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            attackPressedTimer -= Time.deltaTime;

            // Precias setar na animação a variável AttackMovement
            movementController.HandleAttackMovment();

            string nextComboAction = "";
            if (moveIndex + 1 < combos[comboIndex].Moves.Length)
            {
                nextComboAction = combos[comboIndex].Moves[moveIndex + 1].TriggerAction.Name; ;
            }

            //  Check if the nextComboAction was pressed
            if (character.CheckIfActionTriggered(nextComboAction))
            {
                attackPressedTimer = character.AttackPressedTimerWindow;
            }

            // Check if the button was pressed in the attack window            
            if (animator.GetFloat("AttackWindowOpen") > 0.5f && attackPressedTimer >= 0)
            {
                Debug.Log("WindowOpen and AttackPressedTimer");
                shouldCombo = true;
            }

            // If conditions are match, call next move
            if (time >= duration - CurrentMove.ExitEarlier)
            {
                if (shouldCombo && (moveIndex + 1 < combos[comboIndex].Moves.Length))
                {
                    characterStateMachine.MoveIndex = moveIndex + 1;
                    characterStateMachine.SetNextState(characterStateMachine.MoveState);
                }
            }

            if (time >= duration)
            {
                characterStateMachine.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            movementController.EnableMovement();
        }

        // Trigger other actions
        // public override bool OnTrigger(HurtBox target, RaycastHit hit)
        // {
        //     // Debug.Log("OnTrigger AttackState");
        //     // checked if target is a character, fazer lógica para atacar alvos não character depois
        //     if (target.Character == null)
        //         return false;

        //     if (character.TargetsList.Contains(target.Character.gameObject))
        //         return false;

        //     character.Attack(target, hit, comboIndex, attackIndex);
        //     return true;
        // }
    }
}
