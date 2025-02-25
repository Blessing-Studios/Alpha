using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.HealthAndDamage;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Ai
{
    public class AiCharacter : Character
    {
        public float ViewRange = 15.0f;

        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
        public override bool CheckIfActionTriggered(string actionName)
        {
            return true;
        }
        public override bool Hit(IHittable target)
        {
            bool baseValue = base.Hit(target);

            // A IA que bateu vai passar autoridade para o player que tomou o hit
            if (!HasAuthority && target.HasAuthority && baseValue) GetOwnership();

            return baseValue;
        }
        public override void GotHit(IHitter hitter)
        {
            base.GotHit(hitter);
        }
        public void OnAttack(InputActionType triggerAction = null, InputDirectionType triggerDirection = null)
        {
            if (triggerAction == null)
                triggerAction = CharacterStateMachine.GetAllCombos()[0].Moves[0].TriggerAction;

            if (triggerAction == null)
                triggerDirection = CharacterStateMachine.GetAllCombos()[0].Moves[0].TriggerDirection;

            CharacterStateMachine.CharacterState.OnTrigger(triggerAction, triggerDirection);
        }
    }
}

