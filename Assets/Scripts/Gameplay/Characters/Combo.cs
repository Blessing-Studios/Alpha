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
        [Tooltip("What button has to be pressed")][SerializeField][ScriptableObjectDropdown(typeof(ComboActionType))] protected ScriptableObjectReference triggerAction;
        [SerializeField] public InputActionType TriggerAction { get { return triggerAction.value as InputActionType; } }
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
    }
}