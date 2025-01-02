using UnityEngine;
using Blessing.Player;
using Blessing.Scene;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using Blessing.Gameplay.TradeAndInventory;
using Unity.VisualScripting;
using Unity.Netcode;

namespace Blessing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }
        public Camera MainCamera;
        public CinemachineCamera VirtualCamera;
        public List<PlayerCharacter> PlayerCharacterList;
        [field: SerializeField] public List<PlayerController> PlayerList { get; private set; }
        public List<Transform> PlayerSpawnLocations;
        private Dictionary<string, PlayerCharacter> playerCharactersDic = new();
        public Dictionary<string, PlayerCharacter> PlayerCharactersDic { get { return playerCharactersDic; } }
        public SceneStarter SceneStarter;
        public InventoryController InventoryController;
        [field: SerializeField] public GameObject InventoryItemPrefab { get; private set; }
        [field: SerializeField] public NetworkObject LooseItemPrefab { get; private set; }
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
        public void AddPlayer(PlayerController player)
        {
            PlayerList.Add(player);
        }
        public void AddPlayerCharacter(string playerName, PlayerCharacter playerCharacter)
        {
            playerCharactersDic.Add(playerName, playerCharacter);
        }

        public void AddPlayerSpawn(Transform spawn)
        {
            PlayerSpawnLocations.Add(spawn);
        }
        public Vector3 GetPlayerSpawnPosition()
        {
            int index = Random.Range(0, PlayerSpawnLocations.Count);
            return PlayerSpawnLocations[index].position;
        }

        public Transform GetPlayerSpawn(int index)
        {
            return PlayerSpawnLocations[index];
        }

        public void InitializePlayers()
        {
            foreach (PlayerController player in PlayerList)
            {
                player.Initialization();
            }
        }

        public bool ValidateCharOwnerNO(string playerName)
        {
              PlayerCharacter playerCharacter = playerCharactersDic[playerName];
              if (playerCharacter == null) return false;

              if (playerCharacter.GetOwnerName() == playerName) return true;

              return false;
        }

        public void GetOwnership(NetworkObject networkObject)
        {
            ulong LocalClientId = NetworkManager.Singleton.LocalClientId;
            if (LocalClientId != networkObject.OwnerClientId)
                networkObject.ChangeOwnership(LocalClientId);
        }
    }
}

