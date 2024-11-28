using UnityEngine;
using Blessing.Player;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

namespace Blessing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }
        public Camera MainCamera;
        public CinemachineCamera VirtualCamera;
        public List<PlayerCharacter> PlayerCharacterList;
        public List<Transform> PlayerSpawnLocations;
        private Dictionary<string, PlayerCharacter> playerCharactersDic = new();

        public Dictionary<string, PlayerCharacter> PlayerCharactersDic { get { return playerCharactersDic; } }
        protected virtual void Awake()
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
        }

        protected virtual void Start()
        {
            if (MainCamera == null)
            {
                Debug.LogError(base.gameObject.name + ": MainCamera is missing");
            }

            if (VirtualCamera == null)
            {
                Debug.LogError(base.gameObject.name + ": VirtualCamera is missing");
            }
        }

        public void SetSelectedGameObject(GameObject gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void AddPlayerCharacter(string playerName, PlayerCharacter playerCharacter)
        {
            playerCharactersDic.Add(playerName, playerCharacter);
        }
    }
}

