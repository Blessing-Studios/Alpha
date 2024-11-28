using UnityEngine;

namespace Blessing.Gameplay.Characters.InputActions
{
    [CreateAssetMenu(fileName = "InputActionList", menuName = "Scriptable Objects/Characters/InputActionList")]
    public class InputActionList : ScriptableObject
    {
        public InputActionType[] inputActionList;
    }
}
