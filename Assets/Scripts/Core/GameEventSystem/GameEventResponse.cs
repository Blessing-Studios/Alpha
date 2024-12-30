using UnityEngine;
using UnityEngine.Events;

namespace Blessing.Core.GameEventSystem
{
    [System.Serializable]
    public class GameEventResponse : UnityEvent<Component, object> { }
}

