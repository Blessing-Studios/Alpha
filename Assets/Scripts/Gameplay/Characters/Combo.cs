using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;
using System;
using Unity.Netcode;
using Blessing.Gameplay.SkillsAndMagic;

namespace Blessing.Gameplay.Characters
{
    [Serializable] public class Move
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public AnimationClip AnimationClip;
        [SerializeField] public AudioClip[] AudioClips;
        [SerializeField] public string AnimationParam;

        [Tooltip("What button has to be pressed")][ScriptableObjectDropdown(typeof(InputActionType))] public ScriptableObjectReference Action;
        [SerializeField] public InputActionType TriggerAction { get { return Action.value as InputActionType; } }
        [Tooltip("What direction button need to be pressed")][ScriptableObjectDropdown(typeof(InputDirectionType))] public ScriptableObjectReference Direction;
        [SerializeField] public InputDirectionType TriggerDirection { get { return Direction.value as InputDirectionType; } }
        [SerializeField] public float ExitEarlier = 0.1f;
        [SerializeField] public bool CanUseSkill = false; // Teste
    }

    [CreateAssetMenu(fileName = "Combo", menuName = "Scriptable Objects/Combo")]
    public class Combo : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public Move[] Moves;
    }
}