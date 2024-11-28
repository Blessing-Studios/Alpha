using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.Characters.States;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [System.Serializable]
    public class MeleeAttackInfo
    {
        [SerializeField] public AnimationClip AnimationClip;
        [SerializeField] public string AnimationParam;
        [SerializeField] public InputActionList TriggerAction;
        [SerializeField] public InputDirectionList TriggerDirection;
        [SerializeField] public float ExitEarlier = 0.1f;
    }
    [System.Serializable]
    public class ComboInfo
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public MeleeAttackInfo[] AttacksList;
    }
    public class CharacterStateMachine : StateMachine.StateMachine
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public CharacterState CharacterState { get { return CurrentState as CharacterState;} }
        public IdleState IdleState;
        public MoveState MoveState;
        public TakeHitState TakeHitState;
        public DeadState DeadState;

        // public AttackState AttackState;
        // public RangeAttackState RangeAttackState;
        // public CombatEntryState CombatEntryState;

        public int MoveIndex { get; set; }
        public int ComboIndex { get; set; }

        public InputActionType CurrentAction;
        public InputDirectionType CurrentDirectionAction;

        // [field: SerializeField] private InputActionList inputActionList;
        // [field: SerializeField] private InputDirectionList inputDirectionList;

        public Character Character { get; protected set; }
        public Animator Animator { get; set; }
        public CharacterController CharacterController { get; protected set; }
        public MovementController MovementController { get; protected set; }

        [SerializeField] protected Combo[] combos;

        protected override void Awake()
        {
            base.Awake();

            if (Character == null)
                Character = GetComponent<Character>();

            if (CharacterController == null)
                CharacterController = GetComponent<CharacterController>();

            if (Animator == null)
                Animator = GetComponent<Animator>();

            if (MovementController == null)
                MovementController = GetComponent<MovementController>();
        }

        protected override void Start()
        {
            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            TakeHitState = new TakeHitState(this);
            DeadState = new DeadState(this);

            mainStateType = IdleState;
            base.Start();
        }

        public void StartCombo(InputActionType action, InputDirectionType direction)
        {
            Debug.Log(gameObject.name + ": StartCombo");
            CurrentAction = action;
            CurrentDirectionAction = direction;
            
            for (int comboIndex = 0; comboIndex < combos.Length; comboIndex++)
            {
                InputActionType triggerAction = combos[comboIndex].Moves[0].TriggerAction;
                InputDirectionType triggerDirection = combos[comboIndex].Moves[0].TriggerDirection;

                // Tenta achar um golpe considerando a ação e a direção do input
                if (triggerAction == CurrentAction &&
                    triggerDirection.Name != "Any" &&
                    triggerDirection == CurrentDirectionAction)
                {
                    MoveIndex = 0;
                    ComboIndex = comboIndex;
                    SetNextState(MoveState);
                    return;
                }
            }

            
            for (int comboIndex = 0; comboIndex < combos.Length; comboIndex++)
            {
                InputActionType triggerAction = combos[comboIndex].Moves[0].TriggerAction;
                InputDirectionType triggerDirection = combos[comboIndex].Moves[0].TriggerDirection;

                // Caso o não consiga achar um golpe considerando a ação e a direção, tenta achar consierando apenas a ação
                if (triggerAction == CurrentAction &&
                    triggerDirection.Name == "Any")
                {
                    MoveIndex = 0;
                    ComboIndex = comboIndex;
                    SetNextState(MoveState);
                    return;
                }
            }

            
            SetNextStateToMain();
        }
        public Combo[] GetAllCombos()
        {
            return combos;
        }
    }
}