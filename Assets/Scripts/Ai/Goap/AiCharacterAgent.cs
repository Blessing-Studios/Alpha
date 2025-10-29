using Blessing.Ai;
using Blessing.Ai.Goap;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Blessing.AI.Goap
{
    [RequireComponent(typeof(AiCharacter))]
    public class AiCharacterAgent : AiAgent, ICharacterAgent
    {
        [field: SerializeField] public AiCharacter AiCharacter { get; private set; } 
        [field: SerializeField] public Vector3 MinRange { get; private set; }
        public override void Awake()
        {
            base.Awake();
            AiCharacter = GetComponent<AiCharacter>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void Start()
        {
            base.Start();
        }

        public override void FixedUpdate()
        {
            if (AiCharacter.HasAuthority)
                base.FixedUpdate();
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
            // if (ShowDebug) Debug.Log(gameObject.name + ": OnMovementInput");

            // if (input == Vector3.zero)
            // {
            //     AiCharacter.AiMovementController.HandleAiMovement(Vector2.zero);
            // }

            // if (Target != null )
            // {
            //     AiCharacter.MoveToTargetPosition(Target.transform.position);
            // }
        }

        public override void SetTarget(GameObject target)
        {
            Target = target;
        }

        // Actions methods
        private void AttackAction()
        {
            // Checar os tipos de ataques que a IA tem e decidir qual será o melhor ataque

            // Para testar será escolhido combo 0

            AiCharacter.CurrentCombo = AiCharacter.CharacterStateMachine.Combos[0];

            if (ShowDebug) Debug.Log(gameObject.name + ": AttackAction call");

            AiCharacter.OnAttack();
        }
    }

}
