using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public class TakeHitState : CharacterState
    {
        public class StateContext
        {
            // public AttackInfo AttackInfo;
            public Vector3 HitPoint;
        }

        public const float MaxDuration = 2;
        public StateContext Context;
        protected MovementController movementController;
        protected CharacterController characterController;
        protected float attackDamage;
        protected float impulse;
        private float vel;
        private float accel;
        private Vector3 hitPoint;

        // Refazer TakeHitState Class
        public TakeHitState(CharacterStateMachine _characterStateMachine) : base(_characterStateMachine)
        {
            Context = new StateContext();

            // movementController = character.GetMovementController();
            // characterController = movementController.GetCharacterController();

            // Duração não vai ser a da animação, mas vai depender dos estatus dos personagens
            // Vai ser o tempo de recuperar o controle do personagem

            // duration = 1 / MaxDuration + MaxDuration / (1f + character.Constitution + character.Strength);
            duration  = 1f;

            // Debug.Log(duration);
            // foreach (AnimationClip clip in comboStateMachine.gameObject.GetComponent<Animator>().runtimeAnimatorController.animationClips)
            // {
            //     if (clip.name == character.TakeHitAnimation[0])
            //     {
            //         duration = clip.length;
            //     }
            // }
        }
        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("TakeHitState OnEnter");
            // Debug.Log("characterController " + characterController);

            animator.SetTrigger("TakeHit");
            movementController.DisableMovement();

            // attackDamage = Context.AttackInfo.Damage;
            // impulse = Context.AttackInfo.Impulse;

            // Refazer esse sistema
            // switch (Context.AttackInfo.AttackType)
            // {
            //     case AttackTypeList.Light:
            //         vel = 0.0f;
            //         break;
            //     case AttackTypeList.Medium:
            //         vel = impulse / character.Weight;
            //         break;
            //     case AttackTypeList.Heavy:
            //         vel = 2f * impulse / character.Weight;
            //         break;
            //     default:
            //         vel = impulse / character.Weight;
            //         break;
            // }

            accel = -vel / duration;

            hitPoint = Context.HitPoint;

            // Context.AttackInfo = null;
            Context.HitPoint = new Vector3();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (time < duration && vel != 0.0f)
            {

                // Move character in the direction of the attack
                Vector3 direction = character.transform.position - hitPoint;
                direction.y = 0.0f;

                characterController.Move(direction.normalized * (vel + accel * Time.deltaTime / 2) * Time.deltaTime);
                vel = vel + accel * Time.deltaTime;
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
    }
}

