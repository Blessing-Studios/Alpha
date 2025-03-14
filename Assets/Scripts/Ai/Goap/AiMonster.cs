using Blessing.Ai;
using Blessing.Ai.Goap;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Blessing.AI.Goap
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AiCharacter))]
    [RequireComponent(typeof(AiMovementController))]
    public class AiMonster : AiAgent, ICharacterAgent
    {
        private NavMeshAgent navMashAgent;
        private AiCharacter aiCharacter;
        private AiMovementController aiMovementController;
        [field: SerializeField] public Vector3 MinRange { get; private set; }
        public override void Awake()
        {
            base.Awake();
            navMashAgent = GetComponent<NavMeshAgent>();
            aiMovementController = GetComponent<AiMovementController>();
            aiCharacter = GetComponent<AiCharacter>();
            navMashAgent.speed = aiMovementController.CharacterSpeed;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void Start()
        {
            base.Start();
            navMashAgent.updatePosition = false;
            navMashAgent.updateRotation = false;
            navMashAgent.Warp(gameObject.transform.position);
        }
        void OnDrawGizmos()
        {
            if (Target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Target.transform.position, MinRange);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Target.transform.position, 0.125f);
            }
        }

        public override object GetValue(string field)
        {
            // Método temporário
            return true;
        }

        public override void OnActionInput(string input)
        {
            Invoke(input, 0);
        }

        public override void OnMovementInput(Vector3 input)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnMovementInput");

            if (input == Vector3.zero)
            {
                aiMovementController.HandleAiMovement(Vector2.zero);
            }

            if (Target != null)
            {
                if (ShowDebug) Debug.Log(gameObject.name + ": OnMovementInput Target - " + Target.name);
                // TODO: olhar erro SetDestination
                navMashAgent.SetDestination(Target.transform.position);
                if (ShowDebug) Debug.Log(gameObject.name + ": OnMovementInput Target Position - " + Target.transform.position);
                if (ShowDebug) Debug.Log(gameObject.name + ": OnMovementInput Velocity - " + navMashAgent.velocity);
                aiMovementController.HandleAiMovement(new Vector2(navMashAgent.velocity.x, navMashAgent.velocity.z).normalized);
            }

            navMashAgent.nextPosition = gameObject.transform.position;
        }

        public override void SetTarget(GameObject target)
        {
            Target = target;
        }

        // Actions methods
        private void AttackAction()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": AttackAction call");
            aiCharacter.OnAttack();
        }
    }

}
