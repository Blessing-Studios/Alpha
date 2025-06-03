using System.Collections.Generic;
using System.Linq;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class ComboState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;
        protected Combo[] combos;
        protected bool shouldCombo;
        public Move CurrentMove;
        private int moveIndex;
        private int comboIndex;
        protected float attackPressedTimer = 0;
        private List<int> matchedComboIndex = new();
        private InputActionType inputAction;
        private InputDirectionType inputDirection;
        
        public ComboState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {
            movementController = characterStateMachine.MovementController;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!characterStateMachine.Character.HasAuthority) return;

            matchedComboIndex = new List<int>();

            inputAction = null;
            inputDirection = null;

            character.ClearTargetList();

            combos = characterStateMachine.Combos;

            comboIndex = characterStateMachine.ComboMoveIndex.x;
            moveIndex = characterStateMachine.ComboMoveIndex.y;

            // If combo Index is bigger than Length, SetNextStateToMain
            if (comboIndex < 0) characterStateMachine.SetNextStateToMain();

            CurrentMove = combos[comboIndex].Moves[moveIndex];

            // Save move in characterStateMachine
            characterStateMachine.CurrentMove = CurrentMove;

            movementController.DisableMovement();

            // Trigger animation
            networkAnimator.SetTrigger(CurrentMove.AnimationParam);
            
            // Zera movementController.AttackMovement
            movementController.AttackMovement = Vector3.zero;

            duration = characterStateMachine.GetCurrentMoveDuration();

            shouldCombo = false;
            attackPressedTimer = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!characterStateMachine.Character.HasAuthority) return;

            attackPressedTimer -= Time.deltaTime;

            // Precias setar na animação a variável AttackMovement
            movementController.HandleAttackMovement();

            // TODO: Checar erro
            
            foreach(int nextComboIndex in characterStateMachine.MatchedCombosIndex)
            if (moveIndex + 1 < combos[nextComboIndex].Moves.Length)
            {
                if (CheckIfComboMoveTriggered(combos[nextComboIndex].Moves[moveIndex + 1]))
                {
                    attackPressedTimer = character.AttackPressedTimerWindow;
                    comboIndex = nextComboIndex;
                    if (!matchedComboIndex.Contains(nextComboIndex)) matchedComboIndex.Add(nextComboIndex);
                }
            }

            // Check if the button was pressed in the attack window            
            if (animator.GetFloat("AttackWindowOpen") > 0.5f && attackPressedTimer >= 0)
            {
                shouldCombo = true;
            }

            if (!shouldCombo)
            {
                matchedComboIndex.Clear();
            }

            // If conditions are match, call next move
            if (time >= duration - CurrentMove.ExitEarlier)
            {
                if (shouldCombo && (moveIndex + 1 < combos[comboIndex].Moves.Length))
                {
                    characterStateMachine.MatchedCombosIndex = matchedComboIndex;
                    characterStateMachine.SetComboMoveIndex(comboIndex, moveIndex + 1);
                    characterStateMachine.SetNextState(characterStateMachine.ComboState);
                }
            }

            if (time >= duration)
            {
                
                characterStateMachine.SetNextStateToMain();
            }
        }
        public bool CheckIfComboMoveTriggered(Move move)
        {   
            bool checkAction = move.TriggerAction == inputAction;

            bool checkDirection;
            if (move.TriggerDirection == null || move.TriggerDirection.Name == "Any")
                checkDirection = true;
            else
                checkDirection = move.TriggerDirection == inputDirection;

            return checkAction && checkDirection;
        }
        public override bool OnTrigger(InputActionType triggerAction, InputDirectionType triggerDirection)
        {
            inputAction = triggerAction;
            inputDirection = triggerDirection;

            return true;
        }

        public override void OnExit()
        {
            base.OnExit();
            movementController.EnableMovement();
            movementController.AttackMovement = Vector3.zero;
            characterStateMachine.MatchedCombosIndex = matchedComboIndex;
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
