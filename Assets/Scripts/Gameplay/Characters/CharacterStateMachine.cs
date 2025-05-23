using System;
using System.Collections.Generic;
using System.Linq;
using Blessing.Audio;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.Characters.States;
using Blessing.StateMachine;
using Unity.Netcode.Components;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [Serializable] public struct AnimationDuration
    {
        public string Name;
        public float Duration;

        public AnimationDuration(string name, float duration)
        {
            Name = name;
            Duration = duration;
        }
    }
    public class CharacterStateMachine : StateMachine.StateMachine
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public CharacterState CharacterState { get { return CurrentState as CharacterState;} }
        public IdleState IdleState;
        public ComboState ComboState;
        public CastState CastState;
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
        public List<AnimationDuration> AnimationsDuration = new();
        public int ComboIndex { get { return ComboMoveIndex.x;} }
        public int MoveIndex { get { return ComboMoveIndex.y;} }
        public int AbilityIndex;
        [field: SerializeField] public Vector2Int ComboMoveIndex { get; set; }
        [field: SerializeField] public Move CurrentMove { get; set;}
        [SerializeField] protected Combo[] combos;
        public Combo[] Combos { get { return combos; } }
        [SerializeField] protected CastActionType[] castActions;
        public CastActionType[] CastActions { get { return castActions; } }
        public List<int> MatchedCombosIndex = new();
        public bool ActionPressed { get; set; }

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

            IdleState = new IdleState(this, 0);
            ComboState = new ComboState(this, 1);
            CastState = new CastState(this, 2);
            TakeHitState = new TakeHitState(this, 3);
            DeadState = new DeadState(this, 4);

            // TODO: Automatizar essa parte no futuro
            StateList.Add(IdleState);
            StateList.Add(ComboState);
            StateList.Add(CastState);
            StateList.Add(TakeHitState);
            StateList.Add(DeadState);

            mainStateType = IdleState;
        }
        protected override void Start()
        {
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

        public bool StartCombo(ComboActionType action, InputDirectionType direction)
        {
            MatchedCombosIndex.Clear();
            CurrentAction = action;
            CurrentDirectionAction = direction;

            for (int comboIndex = 0; comboIndex < combos.Length; comboIndex++)
            {
                InputActionType triggerAction = combos[comboIndex].Moves[0].TriggerAction;
                InputDirectionType triggerDirection = combos[comboIndex].Moves[0].TriggerDirection;

                // Tenta achar um golpe considerando a ação e a direção do input
                if (triggerAction == CurrentAction &&
                    (triggerDirection == null || triggerDirection.Name == "Any" ||
                    triggerDirection == CurrentDirectionAction))
                {
                    MatchedCombosIndex.Add(comboIndex);
                }
            }

            if (MatchedCombosIndex.Count > 0)
            {
                SetComboMoveIndex(MatchedCombosIndex[^1], 0);
                SetNextState(ComboState);
                return true;
            }

            SetNextStateToMain();
            return false;
        }

        public bool StartCast(CastActionType action)
        {
            for (int i= 0; i < castActions.Length; i++)
            {
                if (castActions[i] == action)
                {
                    if (!Character.CanCastAbility(i)) return false;

                    SetAbilityIndex(i);
                    SetNextState(CastState);
                    return true;
                }
            }

            return false;
        }

        public void SetComboMoveIndex(int comboIndex, int moveIndex)
        {
            ComboMoveIndex = new Vector2Int(comboIndex, moveIndex);
            Character.SetComboMoveIndex(ComboMoveIndex);
        }

        public void SetAbilityIndex(int abilityIndex)
        {
            AbilityIndex = abilityIndex;
            Character.SetAbilityIndex(AbilityIndex);
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
            // ComboIndex = -1 means no CurrentMove
            if (ComboMoveIndex.x < 0)
            {
                CurrentMove = null;
                return;
            } 

            CurrentMove = combos[ComboMoveIndex.x].Moves[ComboMoveIndex.y];
        }
        
#if UNITY_EDITOR
        public UnityEditor.Animations.AnimatorController AnimatorController;
        protected override void OnValidate()
        {
            base.OnValidate();

            AnimationsDuration.Clear();
            foreach (UnityEditor.Animations.AnimatorControllerLayer layer in AnimatorController.layers)
            {
                ValidateStateMachine(layer.stateMachine);
            }
        }

        private void ValidateStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
        {
            foreach (UnityEditor.Animations.ChildAnimatorState child in stateMachine.states)
            {
                if (child.state.motion == null)
                {
                    continue;
                }

                AnimationClip clip =  child.state.motion as AnimationClip;

                if (child.state.motion.averageDuration != clip.length)
                {
                    Debug.LogError(this.name + " - Clip length not matching with Motion average Duration");
                }

                AnimationsDuration.Add(new AnimationDuration(child.state.name, clip.length));
            }

            if (stateMachine.stateMachines.Length > 0)
            {
                foreach(UnityEditor.Animations.ChildAnimatorStateMachine childMachine in stateMachine.stateMachines)
                {
                    ValidateStateMachine(childMachine.stateMachine);
                }
            }
        }
#endif
    }
}