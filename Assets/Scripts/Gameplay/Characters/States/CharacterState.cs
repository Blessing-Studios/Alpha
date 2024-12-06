using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.StateMachine;
using Unity.Netcode.Components;
using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    public abstract class CharacterState : State
    {
        public int StateIndex { get; protected set; }
        // How long this state should be active for before moving on
        public float duration;
        // Cached animator component
        protected Animator animator;
        protected NetworkAnimator networkAnimator;

        // protected AnimationCrontroller animator;
        // bool to check whether or not the next attack in the sequence should be played or not
        protected bool shouldCombo;
        // Player Input System
        // Cached Character class
        protected Character character;
        protected CharacterStateMachine characterStateMachine;

        // The Hit Effect to Spawn on the afflicted Enemy
        private GameObject HitEffectPrefab;

        public CharacterState(CharacterStateMachine _characterStateMachine, int _stateIndex)
        {
            characterStateMachine = _characterStateMachine;
            character = characterStateMachine.Character;
            StateIndex = _stateIndex;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            animator = characterStateMachine.Animator;
            networkAnimator = characterStateMachine.NetworkAnimator;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        public virtual bool OnTrigger(InputActionType triggerAction, InputDirectionType triggerDirection)
        {
            return false;
        }

        /**
        public override bool OnTakeDamage(object sender, System.EventArgs eventArgs)
        {
            int health = character.Health.GetHealth();
            if (health > 0)
                characterStateMachine.SetNextState(characterStateMachine.TakeHitState);

            if (health <= 0)
                characterStateMachine.SetNextState(characterStateMachine.DeadState);

            return true;
        }
        **/
    }
}