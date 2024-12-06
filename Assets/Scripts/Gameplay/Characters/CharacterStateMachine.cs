using System.Collections.Generic;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.Characters.States;
using Blessing.StateMachine;
using Unity.Netcode.Components;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    public class CharacterStateMachine : StateMachine.StateMachine
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public CharacterState CharacterState { get { return CurrentState as CharacterState;} }
        public IdleState IdleState;
        public MoveState MoveState;
        public TakeHitState TakeHitState;
        public DeadState DeadState;
        public List<CharacterState> StateList = new();
        // public AttackState AttackState;
        // public RangeAttackState RangeAttackState;
        // public CombatEntryState CombatEntryState;
        public InputActionType CurrentAction;
        public InputDirectionType CurrentDirectionAction;
        // [field: SerializeField] private InputActionList inputActionList;
        // [field: SerializeField] private InputDirectionList inputDirectionList;

        public Character Character { get; protected set; }
        public Animator Animator { get; private set; }
        public NetworkAnimator NetworkAnimator { get; private set; }
        public CharacterController CharacterController { get; protected set; }
        public MovementController MovementController { get; protected set; }
        public int MoveIndex { get; set; }
        public int ComboIndex { get; set; }
        public Move CurrentMove { get; set;}
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

            if (NetworkAnimator == null)
                NetworkAnimator = GetComponent<NetworkAnimator>();

            if (MovementController == null)
                MovementController = GetComponent<MovementController>();
        }

        protected override void Start()
        {
            IdleState = new IdleState(this, 0);
            MoveState = new MoveState(this, 1);
            TakeHitState = new TakeHitState(this, 2);
            DeadState = new DeadState(this, 3);

            // TODO: Automatizar essa parte no futuro
            StateList.Add(IdleState);
            StateList.Add(MoveState);
            StateList.Add(TakeHitState);
            StateList.Add(DeadState);

            mainStateType = IdleState;

            base.Start();

            // In case IdleState is not the current one, call current State
            // Repensar essa parte
            if (Character.StateIndex != 0)
            {
                SetNextStateByIndex(Character.StateIndex);
            }
        }

        public new void SetNextStateToMain()
        {
            SetNextState(IdleState);
        }

        public void SetNextState(CharacterState characterState)
        {
            if (characterState != null)
            {
                Character.SetStateIndex(characterState.StateIndex);
                nextState = characterState;
            }
        }

        public void SetNextStateByIndex(int index)
        {
            SetNextState(StateList[index]);
        }

        public void StartCombo(InputActionType action, InputDirectionType direction)
        {
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
                    SetNextState((CharacterState) MoveState);
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