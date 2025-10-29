using UnityEngine;
using Blessing.Player;
using Blessing.Scene;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEngine.Pool;
using TMPro;
using Blessing.GameData;
using System;
using Blessing.Gameplay.Characters.Traits;
using Blessing.HealthAndDamage;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine.VFX;
using Blessing.Services;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Blessing.UI.Quests;
using UnityEngine.Rendering.Universal;
using Blessing.Core.ScriptableObjectDropdown;

namespace Blessing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }

        [ScriptableObjectDropdown(typeof(SceneReference), grouping = ScriptableObjectGrouping.ByFolder)]
        [SerializeField] private ScriptableObjectReference mainMenuScene;
        public SceneReference MainMenuScene => mainMenuScene.value as SceneReference;
        public Camera MainCamera;
        public CinemachineCamera VirtualCamera;
        public Archetype[] Archetypes;
        public TraitList AllTraits;
        public ItemList AllItems;
        public List<PlayerCharacter> PlayerCharacterList = new();
        [Tooltip("Player Controller of the current client player")]
        public PlayerController PlayerController;
        private bool playersInitialized = false;
        [field: SerializeField] public List<PlayerController> Players { get; private set; }
        public List<Transform> PlayerSpawnLocations = new();
        public List<SessionOwnerNetworkObjectSpawner> ObjectSpawners = new();
        private Dictionary<string, PlayerCharacter> playerCharactersDic = new();
        public Dictionary<string, PlayerCharacter> PlayerCharactersDic { get { return playerCharactersDic; } }
        public SceneStarter SceneStarter;
        public UIController UIController { get { return UIController.Singleton; } }
        [field: SerializeField] public NetworkObject ContainerPrefab { get; private set; }
        [field: SerializeField] public NetworkObject LooseItemPrefab { get; private set; }
        [Header("Multiplayer")]
        [HideInInspector] public bool PlayerConnected = false;
        [Header("Misc")]

        // Para testar o modo horda, temporário
        public SessionOwnerNetworkObjectSpawner MultiSpawner;
        public int AiCharacterSpawned = 0;
        public ContextDropDownMenu ContextDropDownMenu;
        public ItemInfoBox ItemInfoBox;
        public InventoryItem InventoryItemPrefab;
        public TraderItem TraderItemPrefab;
        public QuestUIElement QuestUIElementPrefab;
        public QuestItem QuestItemPrefab;
        public TextMeshProUGUI SimpleTextPrefab;
        public float GlobalShakeForce = 1f;
        public float GroundGravity = -0.2f;
        public float Gravity = -0.8f;
        [Tooltip("This type of item will be counted as Coin for Trade")][ScriptableObjectDropdown(typeof(ItemType), grouping = ScriptableObjectGrouping.None)]
        [SerializeField] private ScriptableObjectReference coinType;
        public ItemType CoinType { get { return coinType.value as ItemType; } }
        private CinemachineImpulseListener impulseListener;
        private CinemachineVolumeSettings cameraVolumeSettings;
        private DepthOfField depthOfField;
        // Usar originalFocusDistance no Settings
        private float originalFocusDistance = 10f; 
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

            // Se CinemachineImpulseListener não for encontrado vai dar erro
            impulseListener = VirtualCamera.GetComponent<CinemachineImpulseListener>();
            cameraVolumeSettings = VirtualCamera.GetComponent<CinemachineVolumeSettings>();


            cameraVolumeSettings.Profile.TryGet(out depthOfField);
        }

        protected virtual void Start()
        {
            if (MainCamera == null)
                Debug.LogError(base.gameObject.name + ": MainCamera is missing");

            if (VirtualCamera == null)
                Debug.LogError(base.gameObject.name + ": VirtualCamera is missing");

            if (UIController == null)
                Debug.LogError(base.gameObject.name + ": UIController is missing");

            if (CoinType == null)
                Debug.LogError(gameObject.name + ": Missing CoinType");

            if (CoinType == null)
                Debug.LogError(gameObject.name + ": Missing CoinType");

            if (CoinType.name != "Coin")
                Debug.LogError(gameObject.name + ": CoinType is not Coin - " + CoinType.name);

            ContextDropDownMenu.gameObject.SetActive(false);
        }

        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.Tab) && PlayerController != null)
            // {
            //     UIController.ToggleInventoryUI();
            // }

            if (Input.GetKeyDown(KeyCode.P))
            {
                UIController.CreateRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                UIController.InsertRandomItem();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                UIController.RotateItem();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                GameDataManager.Singleton.SaveGame();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                UIController.ConsumeSelectedItem();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                InitializeSpawners();
            }
        }

        public void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            PlayerConnected = true;

            InitializeSpawners();
        }
        public void SetSelectedGameObject(GameObject gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
        public void AddPlayer(PlayerController player)
        {
            Players.Add(player);
        }
        public void AddPlayerCharacter(string playerName, PlayerCharacter playerCharacter)
        {
            // Error: An item with the same key has already been added
            playerCharactersDic.Add(playerName, playerCharacter);
        }
        public void RemovePlayerCharacter(string playerName)
        {
            // TODO: Checar se precisa remover
            // PlayerCharacterList.Remove(playerCharactersDic[playerName]);
            // playerCharactersDic.Remove(playerName);
        }

        public void AddPlayerSpawn(Transform spawn)
        {
            PlayerSpawnLocations.Add(spawn);
        }
        public Vector3 GetPlayerSpawnPosition()
        {
            int index = UnityEngine.Random.Range(0, PlayerSpawnLocations.Count);
            return PlayerSpawnLocations[index].position;
        }

        public Transform GetPlayerSpawn(int index)
        {
            return PlayerSpawnLocations[index];
        }

        public Archetype GetArchetypeById(int id)
        {
            foreach (Archetype archetype in Archetypes)
            {
                if (archetype.Id == id) return archetype;
            }

            return null;
        }

        internal InventoryItem FindInventoryItem(InventoryItemData data, bool createNew = true)
        {
            return UIController.FindInventoryItem(data, createNew);
        }
        public void InitializeSpawners()
        {
            if (PlayerConnected && GameDataManager.Singleton.IsHost && SceneStarter.HasStarted)
            {
                foreach (SessionOwnerNetworkObjectSpawner spawner in ObjectSpawners)
                {
                    spawner.Spawn();
                }
            }
        }

        public void InitializePlayers()
        {

            // if (playersInitialized == true) return;

            if (SceneStarter == null || SceneStarter.HasStarted == false) return;
            if (PlayerController == null || PlayerController.IsReady == false) return;

            foreach (PlayerController player in Players)
            {
                player.Initialization();
            }

            foreach (PlayerCharacter playerCharacter in PlayerCharacterList)
            {
                playerCharacter.TriggerInitializePlayerChar();
            }

            playersInitialized = true;
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

        public bool UpdateLocalList<T>(ref List<T> localList, NetworkList<T> networkList) where T : unmanaged, IEquatable<T>
        {
            List<T> tempList = new();
            foreach (T data in networkList)
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

        public void ClearGameStates()
        {
            playersInitialized = false;
            PlayerConnected = false;
            PlayerController = null;

            playerCharactersDic.Clear();
            Players.Clear();
            PlayerCharacterList.Clear();
            PlayerSpawnLocations.Clear();
            ObjectSpawners.Clear();

            UIController.ClearInventoryItemDic();

            // Reset Camera Position
            VirtualCamera.LookAt = null;
            VirtualCamera.Target.TrackingTarget = null;
            VirtualCamera.transform.position = Vector3.zero;

            // Clean Other Pools
            PoolManager.Singleton.ClearAllPools();
        }
        public void CameraShake(CinemachineImpulseSource impulseSource, CameraShakeEffect shakeEffect = null)
        {
            float force = 1f;

            if (shakeEffect != null)
            {
                // Change Impulse Source Settings
                impulseSource.ImpulseDefinition = shakeEffect.ImpulseDefinition;
                impulseSource.DefaultVelocity = shakeEffect.DefaultVelocity;
                force = shakeEffect.ImpactForce;

                // Change Impulse Listener Settings
                impulseListener.ReactionSettings.AmplitudeGain = shakeEffect.ListenerAmplitude;
                impulseListener.ReactionSettings.FrequencyGain = shakeEffect.ListenerFrequency;
                impulseListener.ReactionSettings.Duration = shakeEffect.ListenerDuration;
            }
            
            impulseSource.GenerateImpulse(GlobalShakeForce * force);
        }


        public void AddBlurToBackground()
        {
            depthOfField.focusDistance.value = 0.2f;   
        }

        public void RemoveBlurFromBackground()
        {
            depthOfField.focusDistance.value = originalFocusDistance; 
        }
    }
}

