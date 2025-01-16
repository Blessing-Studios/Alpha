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

            animator.SetTrigger("Die");

            character.OnDeath();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (time >= duration)
            {
                // comboStateMachine.SetNextStateToMain();
            }
        }
    }
}