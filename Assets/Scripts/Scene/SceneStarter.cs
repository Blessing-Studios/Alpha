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
            // Debug.Log(gameObject.name + " Awake");
            GameManager.Singleton.SceneStarter = this;
        }

        public override void OnNetworkSpawn()
        {
            // if (!HasStarted.Value)
            // {
            //     HasStarted.Value = true;
            //     GameManager.Singleton.InitializePlayers();
            // }
            // GameManager.Singleton.InitializePlayers();
        }
        
        void Start()
        {
            // Debug.Log(gameObject.name + " Start");
        }
    }
}