using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;
using System;
// using UnityEditor.Animations;

namespace Blessing.Gameplay.Characters
{
    [Serializable]
    public class Move
    {
        [SerializeField] public string Name;
        [SerializeField][TextArea] public string Description;
        [SerializeField] public AudioClip[] AudioClips;
        [ScriptableObjectDropdown(typeof(AnimationParamReference))] public ScriptableObjectReference AnimationParamRef;
        [SerializeField] public string AnimationParam { get { return (AnimationParamRef.value as AnimationParamReference).Name; } }
        [SerializeField] public float Duration;
        [Tooltip("What button has to be pressed")][ScriptableObjectDropdown(typeof(InputActionType))] public ScriptableObjectReference Action;
        [SerializeField] public InputActionType TriggerAction { get { return Action.value as InputActionType; } }
        [Tooltip("What direction button need to be pressed")][ScriptableObjectDropdown(typeof(InputDirectionType))] public ScriptableObjectReference Direction;
        [SerializeField] public InputDirectionType TriggerDirection { get { return Direction.value as InputDirectionType; } }
        [SerializeField] public float ExitEarlier = 0.1f;
        [SerializeField] public float DamageMultiplier = 1.0f;
        [SerializeField] public float ImpactMultiplier = 1.0f;
        [Tooltip("Let hit the same target multiple times with the same attack")][SerializeField] public bool CanMultiHit = false;
        [SerializeField] public bool CanUseSkill = false; // Teste
        [SerializeField] public CameraShakeEffect ShakeEffect;

    }

    [CreateAssetMenu(fileName = "Combo", menuName = "Scriptable Objects/Combo")]
    public class Combo : ScriptableObject
    {
        public string Name;
        [TextArea] public string Description;
        
        public Move[] Moves;
        

#if UNITY_EDITOR
        public UnityEditor.Animations.AnimatorController AnimatorController;
        void OnValidate()
        {
            Debug.Log(this.name + " Validate");
            foreach (UnityEditor.Animations.AnimatorControllerLayer layer in AnimatorController.layers)
            {
                ValidateStateMachine(layer.stateMachine);
            }
        }

        private void ValidateStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
        {
            foreach (UnityEditor.Animations.ChildAnimatorState child in stateMachine.states)
            {
                foreach (Move move in Moves)
                {
                    if (child.state.name == move.AnimationParam)
                    {
                        if (child.state.motion == null)
                        {
                            Debug.LogError(this.name + " - Motion not found");
                            continue;
                        }

                        AnimationClip clip =  child.state.motion as AnimationClip;

                        if (child.state.motion.averageDuration != clip.length)
                        {
                            Debug.LogError(this.name + " - Clip length not matching with Motion average Duration");
                        }

                        move.Duration = clip.length;
                    }
                }
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