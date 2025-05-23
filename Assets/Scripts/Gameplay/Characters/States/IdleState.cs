using System.Diagnostics;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;

namespace Blessing.Gameplay.Characters.States
{
    public class IdleState : CharacterState
    {
        public IdleState(CharacterStateMachine _characterStateMachine, int _stateIndex) : base(_characterStateMachine, _stateIndex)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();

            characterStateMachine.CurrentMove = null;
            characterStateMachine.SetComboMoveIndex(-1, -1);

            duration = 1.0f;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // If character is still in idle trigger Die again
            // TODO: criar lógica para ser possível bater em alguém durante a animação de morte
            if (time >= duration && characterStateMachine.Character.Health.IsDead)
            {
                characterStateMachine.Character.OnDeath();
                characterStateMachine.SetNextState(characterStateMachine.DeadState);
            }
        }
        public override bool OnTrigger(InputActionType triggerAction, InputDirectionType triggerDirection)
        {
            if (triggerAction is ComboActionType comboActionType)
            {
                characterStateMachine.StartCombo(comboActionType, triggerDirection);
                return true;
            }
            
            if (triggerAction is CastActionType castActionType)
            {
                characterStateMachine.StartCast(castActionType);
                return true;
            }

            return true;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
