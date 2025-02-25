using System.Collections.Generic;
using Blessing.Audio;
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
        public ComboState ComboState;
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
        public int ComboIndex { get { return ComboMoveIndex.x;} }
        public int MoveIndex { get { return ComboMoveIndex.y;} }
        [field: SerializeField] public Vector2Int ComboMoveIndex { get; set; }
        [field: SerializeField] public Move CurrentMove { get; set;}
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
            ComboState = new ComboState(this, 1);
            TakeHitState = new TakeHitState(this, 2);
            DeadState = new DeadState(this, 3);

            // TODO: Automatizar essa parte no futuro
            StateList.Add(IdleState);
            StateList.Add(ComboState);
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

            UpdateCurrentMove();
        }

        public new void SetNextStateToMain()
        {
            SetNextState(IdleState);
        }

        public void SetNextState(CharacterState characterState)
        {
            if (characterState != null)
            {
                if (ShowDebug) Debug.Log(gameObject.name + ": SetNextState characterState - " + characterState.ToString());
                Character.SetStateIndex(characterState.StateIndex);
                nextState = characterState;
            }
        }

        public void SetNextStateByIndex(int index)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": SetNextStateByIndex StateIndex- " + index);
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
                    triggerDirection != null &&
                    triggerDirection.Name != "Any" &&
                    triggerDirection == CurrentDirectionAction)
                {
                    // MoveIndex = 0;
                    // ComboIndex = comboIndex;
                    SetComboMoveIndex(comboIndex, 0);
                    SetNextState(ComboState);
                    return;
                }
            }

            
            for (int comboIndex = 0; comboIndex < combos.Length; comboIndex++)
            {
                InputActionType triggerAction = combos[comboIndex].Moves[0].TriggerAction;
                InputDirectionType triggerDirection = combos[comboIndex].Moves[0].TriggerDirection;

                // Caso o não consiga achar um golpe considerando a ação e a direção, tenta achar consierando apenas a ação
                if (triggerAction == CurrentAction &&
                    (triggerDirection == null || triggerDirection.Name == "Any"))
                {
                    // MoveIndex = 0;
                    // ComboIndex = comboIndex;
                    SetComboMoveIndex(comboIndex, 0);
                    SetNextState(ComboState);
                    return;
                }
            }
            
            SetNextStateToMain();
        }

        public void SetComboMoveIndex(int comboIndex, int moveIndex)
        {
            ComboMoveIndex = new Vector2Int(comboIndex, moveIndex);
            Character.SetComboMoveIndex(ComboMoveIndex);
        }

        public Combo[] GetAllCombos()
        {
            return combos;
        }

        // public void OnAnimationAttack()
        // {
        //     AudioClip[] attackAudios = combos[ComboIndex].Moves[MoveIndex].AudioClips;

        //     if (combos[ComboIndex].Moves[MoveIndex].SkillSlot != 0)
        //     {
        //         // Trigger Skill
        //     }

        //     if (attackAudios.Length > 0)
        //         AudioManager.Singleton.PlaySoundFx(attackAudios, transform);
        // }

        public void UpdateCurrentMove()
        {
            CurrentMove = combos[ComboMoveIndex.x].Moves[ComboMoveIndex.y];
        }
    }
}