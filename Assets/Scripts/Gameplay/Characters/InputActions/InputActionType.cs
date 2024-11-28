using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Blessing.Gameplay.Characters.InputActions
{
    [CreateAssetMenu(fileName = "InputActionType", menuName = "Scriptable Objects/Characters/InputActionType")]
    public class InputActionType : ScriptableObject
    {
        public string Name;
    }
}
