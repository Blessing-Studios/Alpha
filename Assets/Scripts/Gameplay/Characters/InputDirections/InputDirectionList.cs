using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.InputDirections
{
    [CreateAssetMenu(fileName = "InputDirectionList", menuName = "Scriptable Objects/Characters/InputDirectionList")]
    public class InputDirectionList : ScriptableObject
    {
        public InputDirectionType[] InputDirections;
    }
}
