using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    // Pensar numa lógica para o estado de morto
    public class DeadState : CharacterState
    {
        protected MovementController movementController;

        public DeadState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {
            movementController = characterStateMachine.gameObject.GetComponent<MovementController>();
        }
        public override void OnEnter()
        {
            base.OnEnter();
            if (!characterStateMachine.Character.HasAuthority) return;

            networkAnimator.SetTrigger("Dying");

            character.OnDeath();

            duration = 2.0f;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // If character is still in idle trigger Die again
            // TODO: criar lógica para ser possível bater em alguém durante a animação de morte
            if (time >= duration && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                networkAnimator.SetTrigger("Dying");
            }
        }
    }
}