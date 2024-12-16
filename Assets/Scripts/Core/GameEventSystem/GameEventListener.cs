using UnityEngine;


namespace Blessing.GameEventSystem
{
    [System.Serializable]
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent GameEvent;
        public GameEventResponse Response;
        public void OnEnable()
        {
            GameEvent.RegisterListener(this);
        }

        public void OnDisable()
        {
            GameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(Component sender, object data)
        {
            Response.Invoke(sender, data);
        }
    }
}

