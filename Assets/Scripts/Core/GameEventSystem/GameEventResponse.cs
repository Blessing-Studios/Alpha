using UnityEngine;
using UnityEngine.Events;

namespace Blessing.GameEventSystem
{
    [System.Serializable]
    public class GameEventResponse : UnityEvent<Component, object> { }
}

