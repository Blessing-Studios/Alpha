using Blessing.Ai.Goap;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.Characters.States;
using Blessing.HealthAndDamage;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Blessing.Ai
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AiAgent))]
    [RequireComponent(typeof(AiMovementController))]
    public class AiCharacter : Character
    {
        private NavMeshAgent navMashAgent;
        private AiAgent aiAgent;
        public AiMovementController AiMovementController;
        public Combo CurrentCombo;

        protected override void Awake()
        {
            base.Awake();
            navMashAgent = GetComponent<NavMeshAgent>();
            aiAgent = GetComponent<AiAgent>();
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
            if (HasAuthority)
                navMashAgent.enabled = true;
        }
        public void EnableNavMashAgent(bool enabled = true)
        {
            navMashAgent.enabled = enabled;
        }
        public override bool CheckIfActionTriggered(InputActionType actionType)
        {
            return true;
        }
        public override bool CheckIfDirectionTriggered(InputDirectionType directionType)
        {
            return true;
        }
        public override bool CheckIfComboMoveTriggered(Move move)
        {
            return true;
        }
        public override bool Hit(IHittable target, Vector3 hitPosition)
        {
            bool baseValue = base.Hit(target, hitPosition);

            // A IA que bateu vai passar autoridade para o player que tomou o hit
            if (!HasAuthority && target.HasAuthority && baseValue) GetOwnership();

            return baseValue;
        }
        public void OnAttack(InputActionType triggerAction = null, InputDirectionType triggerDirection = null)
        {
            // Temporário
            if (CurrentCombo == null)
            {
                CurrentCombo = CharacterStateMachine.Combos[0];
            }

            if (CurrentCombo != null && CharacterStateMachine.ComboIndex < 0)
            {
                triggerAction = CurrentCombo.Moves[0].TriggerAction;
                triggerDirection = CurrentCombo.Moves[0].TriggerDirection;
            }
                
            if (CurrentCombo != null && CharacterStateMachine.ComboIndex >= 0)
            {
                if (CharacterStateMachine.Combos[CharacterStateMachine.ComboIndex] == CurrentCombo)
                {
                    if (CharacterStateMachine.MoveIndex + 1 < CurrentCombo.Moves.Length)
                    {
                        triggerAction = CurrentCombo.Moves[CharacterStateMachine.MoveIndex + 1].TriggerAction;
                        triggerDirection = CurrentCombo.Moves[CharacterStateMachine.MoveIndex + 1].TriggerDirection;
                    }
                }

                // Default Case
                if (triggerAction == null)
                    triggerAction = CurrentCombo.Moves[0].TriggerAction;

                if (triggerDirection  == null)
                    triggerDirection = CurrentCombo.Moves[0].TriggerDirection;
            }

            
            CharacterStateMachine.CharacterState.OnTrigger(triggerAction, triggerDirection);

            if (CharacterStateMachine.CurrentMove != null)
                aiAgent.GoapStateMachine.ActionDuration = CharacterStateMachine.GetCurrentMoveDuration();
        }
        public override void OnDeath()
        {
            base.OnDeath();

            // Disable AI
            navMashAgent.enabled = false;
            aiAgent.enabled = false;
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

