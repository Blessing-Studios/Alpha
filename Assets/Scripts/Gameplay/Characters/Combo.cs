using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;
using System;

namespace Blessing.Gameplay.Characters
{
    [Serializable] public class Move
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public AnimationClip AnimationClip;
        [SerializeField] public string AnimationParam;

        [ScriptableObjectDropdown(typeof(InputActionType))] public ScriptableObjectReference Action;
        [SerializeField] public InputActionType TriggerAction { get { return Action.value as InputActionType; } }
        [ScriptableObjectDropdown(typeof(InputDirectionType))] public ScriptableObjectReference Direction;
        [SerializeField] public InputDirectionType TriggerDirection { get { return Direction.value as InputDirectionType; } }
        [SerializeField] public float ExitEarlier = 0.1f;
        [SerializeField] public int Damage = 10;
        [SerializeField] public int Impulse = 60;
    }

    [CreateAssetMenu(fileName = "Combo", menuName = "Scriptable Objects/Combo")]
    public class Combo : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public Move[] Moves;
    }
}