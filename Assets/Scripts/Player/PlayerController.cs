using System;
using System.Collections;
using System.Collections.Generic;
using Blessing.DataPersistence;
using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Transition;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerController : NetworkBehaviour, IDataPersistence
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public NetworkObject CharacterToSpawn;
        public Vector3 SpawnLocation;
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }
        protected NetworkVariable<int> m_TickToSpawnLoot = new NetworkVariable<int>();
        public int SpawnTime = 20;
        [field: SerializeField] private List<InventoryItemData> gears = new();
        private List<InventoryItemData> items = new();
        [Header("Map Travel Info ")]
        public NetworkList<MapTravelData> SceneSessionNetworkList = new NetworkList<MapTravelData>
            (
                new List<MapTravelData>(),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner
            );
        public List<MapTravelData> SceneSessionLocalList = new();

        public void SetPlayerCharacter(PlayerCharacter playerCharacter)
        {
            if (GetPlayerName() != playerCharacter.GetPlayerOwnerName()) return;

            PlayerCharacter = playerCharacter;
        }

        [HideInInspector] public NetworkVariable<FixedString32Bytes> PlayerName = new();
        public string GetPlayerName()
        {
            return PlayerName.Value.ToString();
        }

        public override void OnNetworkSpawn()
        {
            if (ShowDebug) Debug.Log(gameObject.name + " On Network Spawn");
            PlayerName.OnValueChanged += OnPlayerNameValueChanged;
            SceneSessionNetworkList.OnListChanged += OnSceneSessionNetworkListChanged;
        }
        public override void OnNetworkDespawn()
        {
            PlayerName.OnValueChanged -= OnPlayerNameValueChanged;
            SceneSessionNetworkList.OnListChanged -= OnSceneSessionNetworkListChanged;

            StopAllCoroutines();

            GameManager.Singleton.Players.Remove(this);
        }

        private void OnPlayerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            gameObject.name = "Player-" + newValue.ToString();
        }
        private void OnSceneSessionNetworkListChanged(NetworkListEvent<MapTravelData> changeEvent)
        {
            Debug.Log(gameObject.name + ": OnSceneSessionNetworkListChanged");
             
            GameDataManager.Singleton.UpdateSceneSessionDic(changeEvent.Value);

            // if (!UpdateLocalList(ref SceneSessionLocalList, SceneSessionNetworkList)) return;
        }
        protected bool UpdateLocalList(ref List<MapTravelData> localList, NetworkList<MapTravelData> networkList)
        {
            return GameManager.Singleton.UpdateLocalList(ref localList, networkList);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if (ShowDebug) Debug.Log(gameObject.name + " Awake");
            GameManager.Singleton.AddPlayer(this);
        }

        void Start()
        {
            if (ShowDebug) Debug.Log(gameObject.name + " Start");
            if (HasAuthority)
            {
                // Teste DataSystem
                
                GameDataManager.Singleton.UpdatePersistenceObjectsList();
                GameDataManager.Singleton.LoadGame();
                GameManager.Singleton.PlayerController = this;
                GameManager.Singleton.InitializePlayers();
            }

        }

        public void Initialization()
        {
            if (ShowDebug) Debug.Log("Entrou Initialization");
            if (HasAuthority)
            {
                PlayerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);

                if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetPlayerName()))
                {
                    Debug.Log(gameObject.name + ": Found Player");
                    FindPlayerCharacter();
                }
                else // This player doesn't have a Char in this session, Spawn a new Char
                {
                    Debug.Log(gameObject.name + ": Spawn Player");
                    SpawnPlayerCharacter();
                }

                SetPlayer();
            }

            if (!HasAuthority)
            {
                if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetPlayerName()))
                    PlayerCharacter = GameManager.Singleton.PlayerCharactersDic[GetPlayerName()];
            }

            gameObject.name = "Player-" + GetPlayerName();
        }
        private void FindPlayerCharacter()
        {
            if (ShowDebug) Debug.Log("Entrou FindPlayerCharacter()");
            PlayerCharacter = GameManager.Singleton.PlayerCharactersDic[GetPlayerName()];
            PlayerCharacter.GetPlayerCharacterOwnership();
        }

        private void SpawnPlayerCharacter()
        {
            if (ShowDebug) Debug.Log("Entrou SpawnPlayerCharacter()");
            SpawnLocation = GameManager.Singleton.GetPlayerSpawnPosition();

            if (SpawnLocation == null)
                Debug.LogError(gameObject.name + " SpawnLocation is missing.");

            var spawnedNetworkObject = CharacterToSpawn.InstantiateAndSpawn(NetworkManager, ownerClientId: OwnerClientId);

            PlayerCharacter = spawnedNetworkObject.GetComponent<PlayerCharacter>();
            PlayerCharacter.SpawnLocation = SpawnLocation;
            PlayerCharacter.SetPlayerOwnerName(GetPlayerName());

            PlayerCharacter.InitializePlayerChar();

            m_TickToSpawnLoot.Value = SpawnTime;

            StartCoroutine(WaitToSpawnGear());

            // var spawnedNetworkObject = Instantiate(CharacterToSpawn);

            // PlayerCharacter = spawnedNetworkObject.GetComponent<PlayerCharacter>();
            // PlayerCharacter.SpawnLocation = SpawnLocation;
            // PlayerCharacter.SetPlayerOwnerName(GetPlayerName());
            // spawnedNetworkObject.Spawn();

        }

        IEnumerator WaitToSpawnGear()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot.Value);

            SpawnGear();

            StopCoroutine(WaitToSpawnGear());
        }
        private void SpawnGear()
        {
            if (!HasAuthority) return;

            GameDataManager.Singleton.LoadGame();

            Debug.Log(gameObject.name + ": SpawnGear");

            foreach (InventoryItemData gear in gears)
            {
                InventoryItem inventoryItem = GameManager.Singleton.FindInventoryItem(gear);

                PlayerCharacter.Gear.AddEquipment(inventoryItem);
                inventoryItem.transform.SetParent(PlayerCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }

            Inventory lootInventory = PlayerCharacter.Gear.Inventory;
            if (lootInventory == null) return;

            
            foreach (InventoryItemData item in items)
            {
                InventoryItem inventoryItem = GameManager.Singleton.FindInventoryItem(item);

                lootInventory.AddItem(inventoryItem);
                inventoryItem.transform.SetParent(PlayerCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }
        }

        private void SetPlayer()
        {
            GameManager.Singleton.VirtualCamera.LookAt = PlayerCharacter.transform;
            GameManager.Singleton.VirtualCamera.Target.TrackingTarget = PlayerCharacter.transform;
        }
        public bool ValidateOwner()
        {
            return GameDataManager.Singleton.ValidateOwner(GetPlayerName());
        }
        private void OnApplicationQuit()
        {
            GameDataManager.Singleton.SaveGame();
        }
        public void SaveData<T>(ref T gameData) where T : Data
        {
            if (!HasAuthority) return;

            PlayerData playerData = gameData as PlayerData;

            if (ShowDebug) Debug.Log(gameObject.name + " Save from Player");
            playerData.Name = GetPlayerName();

            gears = new();
            foreach (CharacterEquipment equipment in PlayerCharacter.Gear.Equipments)
            {
                if (equipment.InventoryItem != null)
                    gears.Add(equipment.InventoryItem.Data);
            }

            playerData.Gears = gears;



            items = new();

            if (PlayerCharacter.Gear.Inventory != null)
                foreach (InventoryItem inventoryItem in PlayerCharacter.Gear.Inventory.ItemList)
                {
                    items.Add(inventoryItem.Data);
                }

            playerData.Items = items;

            if (ShowDebug) Debug.Log(gameObject.name + " playerData.Name " + playerData.Name);
        }
        public void LoadData<T>(T gameData) where T : Data
        {
            if (!HasAuthority) return;

            if (ShowDebug) Debug.Log(gameObject.name + " Load to Player");

            PlayerData playerData = gameData as PlayerData;

            gears = playerData.Gears;
            items = playerData.Items;

            if (ShowDebug) Debug.Log(gameObject.name + " playerData.Name " + playerData.Name);
        }

        public void UpdateSceneSessionList(SceneReference scene, string session)
        {
            for (int i = 0; i < SceneSessionNetworkList.Count; i++)
            {
                if (SceneSessionNetworkList[i].Scene == scene.SceneName)
                {
                    SceneSessionNetworkList.RemoveAt(i);
                    // SceneSessionLocalList.RemoveAt(i);
                    break;
                }
            }

            MapTravelData data = new(scene.SceneName, session);
            SceneSessionNetworkList.Add(data);
            // SceneSessionLocalList.Add(data);
        }
    }
}

