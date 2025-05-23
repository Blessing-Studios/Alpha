using System;
using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Blessing.Scene
{
    // Separar em duas classes
    public class SceneStarter : MonoBehaviour
    {
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference MapScene;
        public SceneReference Scene { get { return MapScene.value as SceneReference; } }
        public bool HasStarted = false;

        void Awake()
        {
            GameManager.Singleton.SceneStarter = this;
            SceneManager.Singleton.CurrentScene = Scene;
        }
        
        void Start()
        {
            HasStarted = true;

            GameManager.Singleton.InitializeSpawners();
            
            GameManager.Singleton.InitializePlayers();
        }
    }
}