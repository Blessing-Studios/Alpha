using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Core.GameEventSystem
{
    [CreateAssetMenu(menuName = "GameEvent")]
    public class GameEvent : ScriptableObject
    {
        public List<GameEventListener> Listeners = new();

        // Raise event through different methods signatures
        public void Raise(Component sender, object data = null)
        {
            foreach (GameEventListener listener in Listeners)
            {
                listener.OnEventRaised(sender, data);
            }
        }

        // Manage Listeners
        public void RegisterListener(GameEventListener listener)
        {
            if (!Listeners.Contains(listener))
                Listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (Listeners.Contains(listener))
                Listeners.Remove(listener);
        }
    }
}

