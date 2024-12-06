using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Blessing.Scene
{
    public class SceneStarter : NetworkBehaviour
    {
        public NetworkVariable<bool> HasStarted = new(false);
        void Start()
        {
            GameManager.Singleton.SceneStarter = this;
            
            if (!HasStarted.Value)
            {
                HasStarted.Value = true;
                GameManager.Singleton.InitializePlayers();
            }
        }
    }
}