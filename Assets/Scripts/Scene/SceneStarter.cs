using System;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Blessing.Scene
{
    // Separar em duas classes
    public class SceneStarter : MonoBehaviour
    {
        public bool HasStarted = false;

        void Awake()
        {
            // Debug.Log(gameObject.name + " Awake");
            GameManager.Singleton.SceneStarter = this;
        }
        
        void Start()
        {
            Debug.Log(gameObject.name + ": SceneStarter Start" );
            HasStarted = true;
            GameManager.Singleton.InitializePlayers();
        }
    }
}