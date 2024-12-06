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
        }

        public override bool OnTrigger(InputActionType triggerAction, InputDirectionType triggerDirection)
        {
            characterStateMachine.StartCombo(triggerAction, triggerDirection);

            return true;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
