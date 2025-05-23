using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class TakeHitState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;
        

        // Refazer TakeHitState Class
        public TakeHitState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {
            movementController = characterStateMachine.MovementController;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            duration = 2.0f / characterStateMachine.Character.Stats.Dexterity;

            movementController.DisableMovement();

            networkAnimator.SetTrigger("TakeHit");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (time >= duration)
            {
                if (characterStateMachine.Character.Health.IsAlive)
                    characterStateMachine.SetNextStateToMain();
                else
                    characterStateMachine.SetNextState(characterStateMachine.DeadState);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            movementController.EnableMovement();
        }
    }
}

