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
    [RequireComponent(typeof(PlayerHUD))]
    public class PlayerController : NetworkBehaviour, IDataPersistence
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        private CharacterData characterData;
        public Vector3 SpawnLocation;
        public PlayerHUD PlayerHUD;
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }
        protected NetworkVariable<int> m_TickToSpawnLoot = new NetworkVariable<int>();
        public int SpawnTime = 20;
        private bool hasStarted = false;
        private bool hasSpawned = false;
        public bool WasInitialize = false;
        public bool IsReady { get { return hasStarted && hasSpawned;}}

        // Para debugar
        [field: SerializeField] private List<InventoryItemData> gears = new();
        [field: SerializeField] private List<InventoryItemData> backpackItems = new();
        [field: SerializeField] public List<InventoryItemDataList> utilityItems = new();
        [field: SerializeField] private int rankScore;
        [field: SerializeField] private int rankStrike;
        [field: SerializeField] private List<int> questsCompleted = new();
        [field: SerializeField] private List<int> questsActive = new();

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

            hasSpawned = true;

            InitializePlayers();
        }
        public override void OnNetworkDespawn()
        {
            PlayerName.OnValueChanged -= OnPlayerNameValueChanged;
            SceneSessionNetworkList.OnListChanged -= OnSceneSessionNetworkListChanged;

            StopAllCoroutines();

            GameManager.Singleton.Players.Remove(this);

            GameDataManager.Singleton.SaveGame();
        }

        private void OnPlayerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            gameObject.name = "Player-" + newValue.ToString();
        }
        private void OnSceneSessionNetworkListChanged(NetworkListEvent<MapTravelData> changeEvent)
        {
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

            if (GameDataManager.Singleton.CharacterSelected == null)
            {
                Debug.LogError(gameObject.name + ": CharacterSelected is missing on GameDataManager");
            }


            GameManager.Singleton.AddPlayer(this);

            characterData = GameDataManager.Singleton.CharacterSelected;

            PlayerHUD = GetComponent<PlayerHUD>();
        }
        void Start()
        {
            if (ShowDebug) Debug.Log(gameObject.name + " Start");
            if (HasAuthority)
            {   
                GameDataManager.Singleton.UpdatePersistenceObjectsList();
                GameDataManager.Singleton.LoadGame();
                GameManager.Singleton.PlayerController = this;
            }

            hasStarted = true;

            InitializePlayers();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ShowDebug = !ShowDebug;
            }
        }

        private void InitializePlayers()
        {
            if (IsReady)
                GameManager.Singleton.InitializePlayers();
        }

        public void Initialization()
        {
            if (WasInitialize) return;
            if (ShowDebug) Debug.Log("Entrou Initialization");
            if (HasAuthority)
            {
                PlayerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);

                if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetPlayerName()))
                {
                    FindPlayerCharacter();
                }
                else // This player doesn't have a Char in this session, Spawn a new Char
                {
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

            WasInitialize = true;
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

            if (characterData == null)
            {
                Debug.LogError(gameObject.name + ": characterData can't be null to spawn character");
            }

            Archetype archetype = GameManager.Singleton.GetArchetypeById(characterData.ArchetypeId);

            var spawnedNetworkObject = archetype.Prefab.InstantiateAndSpawn(NetworkManager, ownerClientId: OwnerClientId);

            PlayerCharacter = spawnedNetworkObject.GetComponent<PlayerCharacter>();
            PlayerCharacter.SpawnLocation = SpawnLocation + Vector3.up;
            PlayerCharacter.SetPlayerOwnerName(GetPlayerName());

            PlayerCharacter.InitializePlayerChar();
            PlayerCharacter.Adventurer.Initialize(characterData.RankScore, characterData.RankStrike, characterData.QuestsCompleted, characterData.QuestsActive);

            PlayerCharacter.Network.CharacterName.Value = new FixedString32Bytes(characterData.Name);

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

            foreach (InventoryItemData gear in characterData.Gears)
            {
                InventoryItem inventoryItem = GameManager.Singleton.FindInventoryItem(gear);

                PlayerCharacter.Gear.AddEquipment(inventoryItem);
                inventoryItem.transform.SetParent(PlayerCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }

            Inventory lootInventory = PlayerCharacter.Gear.Inventory;
            if (lootInventory != null)
            {
                foreach (InventoryItemData item in characterData.BackpackItems)
                {
                    InventoryItem inventoryItem = GameManager.Singleton.FindInventoryItem(item);

                    lootInventory.AddItem(inventoryItem);
                    inventoryItem.transform.SetParent(PlayerCharacter.transform, false);
                    inventoryItem.gameObject.SetActive(false);
                }
            }
            
            if (characterData.UtilityItems.Count > 0 && PlayerCharacter.Gear.UtilityInventories.Count == characterData.UtilityItems.Count)
            {
                for(int i = 0; i < characterData.UtilityItems.Count; i++)
                {
                    foreach(InventoryItemData item in characterData.UtilityItems[i].Items)
                    {
                        InventoryItem inventoryItem = GameManager.Singleton.FindInventoryItem(item);
                        PlayerCharacter.Gear.UtilityInventories[i].AddItem(inventoryItem);
                        inventoryItem.transform.SetParent(PlayerCharacter.transform, false);
                        inventoryItem.gameObject.SetActive(false);
                    }
                }
            }
            
            if (characterData.UtilityItems.Count > 0 && PlayerCharacter.Gear.UtilityInventories.Count != characterData.UtilityItems.Count)
            {
                Debug.LogError(gameObject.name + ": Utility Inventories number are not matching in the save file");
            }
        }

        private void SetPlayer()
        {
            PlayerHUD.Initialize(PlayerCharacter);

            UIController.Singleton.SetPlayerCharacter(PlayerCharacter);

            GameManager.Singleton.VirtualCamera.LookAt = PlayerCharacter.transform;
            GameManager.Singleton.VirtualCamera.Target.TrackingTarget = PlayerCharacter.transform;
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

            int characterIndex = -1;
            for (int i = 0; i < playerData.Characters.Count; i++)
            {
                if (playerData.Characters[i].Id == characterData.Id)
                {
                    characterIndex = i;
                    break;
                }
            }

            if (characterIndex == -1)
            {
                Debug.LogError(gameObject + ": characterData not found in save file");
                return;
            }

            characterData.Gears = new();
            foreach (CharacterEquipment equipment in PlayerCharacter.Gear.Equipments)
            {
                if (equipment.InventoryItem != null)
                    characterData.Gears.Add(equipment.InventoryItem.Data);
            }

            playerData.Characters[characterIndex].Gears = characterData.Gears;

            characterData.BackpackItems = new();

            if (PlayerCharacter.Gear.Inventory != null)
            {
                foreach (InventoryItem inventoryItem in PlayerCharacter.Gear.Inventory.ItemList)
                {
                    characterData.BackpackItems.Add(inventoryItem.Data);
                }
            }

            characterData.UtilityItems = new();

            if (PlayerCharacter.Gear.UtilityInventories != null)
            {
                foreach(Inventory inventory in PlayerCharacter.Gear.UtilityInventories)
                {
                    InventoryItemDataList utiInventory = new();
                    utiInventory.Items = new();
                    if (inventory != null)
                    {
                        foreach (InventoryItem inventoryItem in inventory.ItemList)
                        {
                            utiInventory.Items.Add(inventoryItem.Data);
                        }
                    }
                    characterData.UtilityItems.Add(utiInventory);
                }
            }

            playerData.Characters[characterIndex].BackpackItems = characterData.BackpackItems;
            playerData.Characters[characterIndex].UtilityItems = characterData.UtilityItems;
            playerData.Characters[characterIndex].RankScore = PlayerCharacter.Adventurer.Rank.Score;
            playerData.Characters[characterIndex].RankStrike = PlayerCharacter.Adventurer.Rank.Strike;
            playerData.Characters[characterIndex].QuestsCompleted = PlayerCharacter.Adventurer.QuestsCompleted;
            playerData.Characters[characterIndex].QuestsActive = PlayerCharacter.Adventurer.QuestsActive;

            if (ShowDebug) Debug.Log(gameObject.name + " characterData Save finished " + characterData.Name);
        }
        public void LoadData<T>(T gameData) where T : Data
        {
            if (!HasAuthority) return;

            if (ShowDebug) Debug.Log(gameObject.name + " Load to Player");

            PlayerData playerData = gameData as PlayerData;

            foreach(CharacterData data in playerData.Characters)
            {
                if (data.Id == characterData.Id)
                {
                    gears = data.Gears;
                    backpackItems = data.BackpackItems;
                    utilityItems = data.UtilityItems;
                    rankScore = data.RankScore;
                    rankStrike = data.RankStrike;
                    questsCompleted = data.QuestsCompleted;
                    questsActive = data.QuestsActive;

                    characterData = data;

                    if (ShowDebug) Debug.Log(gameObject.name + " characterData Load finished " + data.Name);

                    return;
                }
            }

            // Send error mensage about not finding characterData in save file
            Debug.LogError(gameObject.name + ": CharacterData not found in save file");
        }
    }
}

