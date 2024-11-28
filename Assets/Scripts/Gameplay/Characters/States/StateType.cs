using UnityEngine;

namespace Blessing.Gameplay.Characters.States
{
    [CreateAssetMenu(fileName = "StateType", menuName = "Scriptable Objects/Characters/StateType")]
    public class StateType : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
    }
}