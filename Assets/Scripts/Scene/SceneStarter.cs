using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Blessing.Scene
{
    // Separar em duas classes
    public class SceneStarter : NetworkBehaviour
    {
        public NetworkVariable<bool> HasStarted = new(false);
        void Awake()
        {
            GameManager.Singleton.SceneStarter = this;
        }

        void Start()
        {
            if (!HasStarted.Value)
            {
                HasStarted.Value = true;
                GameManager.Singleton.InitializePlayers();
            }
        }
    }
}