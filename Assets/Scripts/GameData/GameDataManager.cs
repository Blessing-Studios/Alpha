using UnityEngine;
using Blessing.DataPersistence;

namespace Blessing.GameData
{
    public class GameDataManager : DataManager<PlayerData>
    {
        public static GameDataManager Singleton { get; private set; }
        
        public bool IsHost = false; // Temporarário
        public string PlayerName = "";

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

        public bool ValidateOwner(string playerName)
        {
            return PlayerName == playerName;
        }
    }
}
