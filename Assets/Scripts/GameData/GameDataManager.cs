using UnityEngine;
using Blessing.DataPersistence;
using System;
using System.Collections.Generic;
using UnityEngine.Diagnostics;
using Blessing.Player;
using Blessing.Gameplay.Transition;

namespace Blessing.GameData
{
    public class GameDataManager : DataManager<PlayerData>
    {
        public static GameDataManager Singleton { get; private set; }
        public bool IsHost = false; // Temporarário
        // public string SessionName = "";
        public string PlayerName = "";
        private Dictionary<string, string> sceneSessionDic = new();
        public List<string> SceneList = new();
        public List<string> SessionList = new();

        void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Singleton = this;
            }

            if (string.IsNullOrEmpty(dataDirPath))
            {
                dataDirPath = Application.persistentDataPath;
            }

            IsHost =  false;
        }

        public string GetSessionByScene(SceneReference scene)
        {
            // TODOL: criar lógica para código da session

            if (sceneSessionDic.ContainsKey(scene.SceneName))
                return sceneSessionDic[scene.SceneName];

            string newSessionName = Guid.NewGuid().ToString()[..8];

            GameManager.Singleton.PlayerController.UpdateSceneSessionList(scene, newSessionName);

            return newSessionName;
        }
        public void UpdateSceneSessionDic(MapTravelData data)
        {
            sceneSessionDic[data.Scene.ToString()] = data.Session.ToString();

            SceneList.Clear();
            SessionList.Clear();
            
            foreach(KeyValuePair<string, string> item in sceneSessionDic)
            {
                SceneList.Add(item.Key);
                SessionList.Add(item.Value);
            }
        }

        public bool ValidateOwner(string playerName)
        {
            return PlayerName == playerName;
        }
    }
}
