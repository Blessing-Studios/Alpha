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
using UnityEngine.Pool;
using TMPro;
using Blessing.GameData;

namespace Blessing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }
        public Camera MainCamera;
        public CinemachineCamera VirtualCamera;
        public TextMeshProUGUI FpsDisplay;
        public List<PlayerCharacter> PlayerCharacterList;
        [field: SerializeField] public List<PlayerController> PlayerList { get; private set; }
        public List<Transform> PlayerSpawnLocations;
        private Dictionary<string, PlayerCharacter> playerCharactersDic = new();
        public Dictionary<string, PlayerCharacter> PlayerCharactersDic { get { return playerCharactersDic; } }
        public SceneStarter SceneStarter;
        public InventoryController InventoryController;
        public InventoryItemPooler InventoryItemPooler;
        public IObjectPool<InventoryItem> InventoryItemPool;
        [field: SerializeField] public GameObject InventoryItemPrefab { get; private set; }
        [field: SerializeField] public NetworkObject ContainerPrefab { get; private set; }
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

            if (InventoryController == null)
            {
                Debug.LogError(base.gameObject.name + ": InventoryController is missing");
            }

            if (InventoryItemPooler == null)
                Debug.LogError(gameObject.name + ": Missing InventoryItemPooler");

            InventoryItemPool = InventoryItemPooler.Pool;
        }

        private float pollingTime = 0.5f;
        private float time;
        private int frameCount;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                InventoryController.ToggleGrids();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                InventoryController.CreateRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                InventoryController.InsertRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                InventoryController.RotateItem();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                GameDataManager.Singleton.SaveGame();
            }
            

            time += Time.deltaTime;
            frameCount++;

            if (time >= pollingTime)
            {
                FpsDisplay.text = "FPS " + Mathf.RoundToInt(frameCount / time);

                time -= pollingTime;
                frameCount = 0;
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

        public InventoryItem GetInventoryItem()
        {
            return InventoryItemPool.Get();
        }

        internal InventoryItem FindInventoryItem(InventoryItemData data)
        {
            return InventoryController.FindInventoryItem(data);
        }

        public void InitializePlayers()
        {
            foreach(PlayerCharacter playerCharacter in PlayerCharacterList)
            {
                playerCharacter.Initialize();
            }

            foreach (PlayerController player in PlayerList)
            {
                player.Initialization();
            }
        }

        public bool ValidateCharPlayerOwner(string playerName)
        {
            PlayerCharacter playerCharacter = playerCharactersDic[playerName];
            if (playerCharacter == null) return false;

            if (playerCharacter.GetPlayerOwnerName() == playerName) return true;

            return false;
        }

        public void GetOwnership(NetworkObject networkObject)
        {
            ulong LocalClientId = NetworkManager.Singleton.LocalClientId;
            if (LocalClientId != networkObject.OwnerClientId)
                networkObject.ChangeOwnership(LocalClientId);
        }

        public bool UpdateLocalList(ref List<InventoryItemData> localList, NetworkList<InventoryItemData> networkList)
        {
            List<InventoryItemData> tempList = new();
            foreach (InventoryItemData data in networkList)
            {
                tempList.Add(data);
            }

            bool isEqual = true;

            int networkListCount = tempList.Count;
            int localListCount = localList.Count;

            // First check if the lists has diferent sizes before checking each item
            if (networkListCount != localListCount)
            {
                isEqual = false;
            }
            else
            {
                for (int i = 0; i < networkListCount; i++)
                {
                    if (!tempList[i].Equals(localList[i]))
                    {
                        isEqual = false;
                    }
                }
            }

            // If there was no change in the data, do nothing
            if (isEqual)
            {
                return false;
            }

            // Update InventoryLocalList
            localList.Clear();
            localList = tempList;

            return true;
        }
    }
}

