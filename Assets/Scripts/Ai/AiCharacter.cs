using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.HealthAndDamage;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Blessing.Ai
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AiMovementController))]
    public class AiCharacter : Character
    {
        private NavMeshAgent navMashAgent;
        public AiMovementController AiMovementController;

        public float ViewRange = 15.0f;

        protected override void Awake()
        {
            base.Awake();
            navMashAgent = GetComponent<NavMeshAgent>();
            AiMovementController = GetComponent<AiMovementController>();
        }
        public override void Initialize()
        {
            // Initialize é chamada antes do Start no Spawn()
            base.Initialize();

            // Setar navMashAgent
            
            navMashAgent.speed = AiMovementController.CharacterSpeed;
            navMashAgent.Warp(gameObject.transform.position);
            navMashAgent.updatePosition = false;
            navMashAgent.updateRotation = false;
            navMashAgent.enabled = false;
        }
        protected override void Start()
        {
            base.Start();
            navMashAgent.enabled = true;
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

        public void MoveToTargetPosition(Vector3 targetPosition)
        {
            if (navMashAgent.isActiveAndEnabled)
            {
                if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                {
                    navMashAgent.SetDestination(hit.position);
                }
                
                if (ShowDebug) Debug.Log(gameObject.name + ": Walk to Target Position - " + targetPosition);
                if (ShowDebug) Debug.Log(gameObject.name + ": Velocity - " + navMashAgent.velocity);
                AiMovementController.HandleAiMovement(new Vector2(navMashAgent.velocity.x, navMashAgent.velocity.z).normalized);
            }

            navMashAgent.nextPosition = gameObject.transform.position;
        }
    }
}

