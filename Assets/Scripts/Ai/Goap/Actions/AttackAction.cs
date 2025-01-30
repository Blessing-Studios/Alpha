using Blessing.AI.Goap;
using UnityEngine;

namespace Blessing.Ai.Goap.Actions
{
    public class AttackAction : BaseAction
    {
        private bool finished = false;
        private const bool requiresInRange = true;
        public AttackAction(GoapStateMachine goapStateMachine) : base(goapStateMachine)
        {
            AddEffect("AttackTarget", true);

            Cost = 3f;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (time >= duration)
            {
                if (CheckPositionForAction())
                {
                    Perform(gameObject);
                }
                
                finished = true;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            finished = true;
        }

        /** 
        * Reset any variables that need to be reset before planning happens again. 
        */
        public override void Reset()
        {
            finished = false;
            inRange = false;
        }
        /** 
        * Is the action done? 
        */
        public override bool IsDone()
        {
            return finished;
        }

        /** 
        * Procedurally check if this action can run. Not all actions 
        * will need this, but some might. 
        */
        public override bool CheckProceduralPrecondition(GameObject agent)
        {

            if (aiAgent.Target == null)
            {
                return false;
            }

            Target = aiAgent.Target;
            return true;
        }
        /** 
        * Run the action. 
        * Returns True if the action performed successfully or false 
        * if something happened and it can no longer perform. In this case 
        * the action queue should clear out and the goal cannot be reached. 
        */
        public override bool Perform(GameObject agent)
        {
            aiAgent.OnActionInput("AttackAction");
            return true;
        }
        /** 
        * Does this action need to be within range of a target game object? 
        * If not then the moveTo state will not need to run for this action. 
        */
        public override bool RequiresInRange()
        {
            return requiresInRange;
        }
        public override Vector3 SetMinRange()
        {
            ICharacterAgent characterAgent = (ICharacterAgent) aiAgent; ;

            // // Setar no bot um range mínimo para atirar
            minRange = characterAgent.MinRange;

            return minRange;
        }

        public override Vector3 SetTargetPosition()
        {
            // Debug.Log(Target.name);
            // Debug.Log("SetTargetPosition: " + Target.transform.position);

            if (aiAgent.Target == null)
            {
                return TargetPosition = aiAgent.transform.position;
            }

            return TargetPosition = Target.transform.position;
        }

        public override void HandleActionMove(Vector3 direction)
        {
            // Debug.Log("AttackAction handleActionMove");
            aiAgent.OnMovementInput(direction);
        }
    }
}

