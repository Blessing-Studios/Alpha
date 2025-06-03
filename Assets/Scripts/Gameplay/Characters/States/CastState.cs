using System.Collections.Generic;
using System.Linq;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Blessing.Gameplay.Characters.States
{
    public class CastState : CharacterState
    {
        protected MovementController movementController;
        protected CharacterController characterController;
        protected float attackPressedTimer = 0;
        private InputActionType inputAction;
        private InputDirectionType inputDirection;
        public CharacterAbility CurrentAbility;
        private bool canCharge;
        private bool isCharging;

        public CastState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {
            movementController = characterStateMachine.MovementController;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (!characterStateMachine.Character.HasAuthority) return;

            inputAction = null;
            inputDirection = null;

            CurrentAbility = characterStateMachine.Character.Abilities[characterStateMachine.AbilityIndex];

            movementController.DisableMovement();

            // Trigger animation
            networkAnimator.SetTrigger(CurrentAbility.Ability.AnimationParam);

            // Zera movementController.AttackMovement
            movementController.AttackMovement = Vector3.zero;

            // Pegar duração
            duration = characterStateMachine.AnimationsDuration.First(e => e.Name == CurrentAbility.Ability.AnimationParam).Duration;

            attackPressedTimer = 0;
            isCharging = false;
            canCharge = CurrentAbility.Ability.CanCharge;

            // Can't Charge if there is not End Animation
            if (CurrentAbility.Ability.EndAnimationParam == null)
                canCharge = false;

            inputAction = CurrentAbility.CastAction;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!characterStateMachine.Character.HasAuthority) return;

            attackPressedTimer -= Time.deltaTime;

            // Precias setar na animação a variável AttackMovement
            movementController.HandleAttackMovement();

            // Can't Charge Ability
            // Dispara Skill no fim da duração
            // Espera animation de Cast terminar para trigar

            // Can Charge Ability
            // Checa se o botão está sendo segurado até o fim
            // Se Está sendo segurado, Entra na Animação 

            if (canCharge)
            {
                if (!isCharging)
                {
                    if (time >= duration)
                    {
                        isCharging = true;
                    }
                }
                else
                {
                    if (!CheckIfHoldingAction())
                    {
                        networkAnimator.SetTrigger(CurrentAbility.Ability.EndAnimationParam);
                        duration = characterStateMachine.AnimationsDuration.First(e => e.Name == CurrentAbility.Ability.EndAnimationParam).Duration;
                        isCharging = false;
                        canCharge = false;
                        time = 0.0f;
                    }
                }
            }
            else
            {
                if (time >= duration)
                {
                    characterStateMachine.SetNextStateToMain();
                }
            }
        }

        public bool CheckIfHoldingAction()
        {
            return inputAction == CurrentAbility.CastAction;
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
