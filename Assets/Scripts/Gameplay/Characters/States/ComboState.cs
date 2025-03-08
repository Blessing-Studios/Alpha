using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class ComboState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;
        protected Combo[] combos;
        public Move CurrentMove;
        private int moveIndex;
        private int comboIndex;
        protected float attackPressedTimer = 0;
        public ComboState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {
            movementController = characterStateMachine.MovementController;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!characterStateMachine.Character.HasAuthority) return;

            character.ClearTargetList();
            
            combos = characterStateMachine.GetAllCombos();
            
            comboIndex = characterStateMachine.ComboMoveIndex.x;
            moveIndex = characterStateMachine.ComboMoveIndex.y;
            
            CurrentMove = combos[comboIndex].Moves[moveIndex];

            // Save move in characterStateMachine
            characterStateMachine.CurrentMove = CurrentMove;

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
            if (!characterStateMachine.Character.HasAuthority) return;

            attackPressedTimer -= Time.deltaTime;

            // Precias setar na animação a variável AttackMovement
            movementController.HandleAttackMovment();

            string nextComboAction = "";

            if (moveIndex + 1 < combos[comboIndex].Moves.Length)
            {
                nextComboAction = combos[comboIndex].Moves[moveIndex + 1].TriggerAction.Name;
            }

            //  Check if the nextComboAction was pressed
            if (character.CheckIfActionTriggered(nextComboAction))
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
                if (shouldCombo && (moveIndex + 1 < combos[comboIndex].Moves.Length))
                {
                    characterStateMachine.SetComboMoveIndex(comboIndex, moveIndex + 1);
                    characterStateMachine.SetNextState(characterStateMachine.ComboState);
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
