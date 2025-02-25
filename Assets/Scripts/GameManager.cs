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
using Blessing.Gameplay.HealthAndDamage;
using Blessing.Gameplay.SkillsAndMagic;
using UnityEngine.VFX;

namespace Blessing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton { get; private set; }
        public Camera MainCamera;
        public CinemachineCamera VirtualCamera;
        public TextMeshProUGUI FpsDisplay;
        public TraitList AllTraits;
        public List<PlayerCharacter> PlayerCharacterList;
        [field: SerializeField] public List<PlayerController> PlayerList { get; private set; }
        public List<Transform> PlayerSpawnLocations;
        private Dictionary<string, PlayerCharacter> playerCharactersDic = new();
        public Dictionary<string, PlayerCharacter> PlayerCharactersDic { get { return playerCharactersDic; } }
        public SceneStarter SceneStarter;
        public InventoryController InventoryController;
        [field: SerializeField] public NetworkObject ContainerPrefab { get; private set; }
        [field: SerializeField] public NetworkObject LooseItemPrefab { get; private set; }
        [Header("Pooling")]
        public InventoryItemPooler InventoryItemPooler;
        public IObjectPool<InventoryItem> InventoryItemPool;
        public DamageNumberPooler DamageNumberPooler;
        public IObjectPool<DamageNumber> DamageNumberPool;
        public ProjectilePooler ProjectilePooler;
        public IObjectPool<Projectile> ProjectilePool;
        [Header("Multiplayer")]
        public bool PlayerConnected = false;
        [Header("Misc")]
        public float GroundGravity = -0.2f;
        public float Gravity = -0.8f;
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

            if (DamageNumberPooler == null)
                Debug.LogError(gameObject.name + ": Missing DamageNumberPooler");

            // if (ProjectilePooler == null)
            //     Debug.LogError(gameObject.name + ": Missing ProjectilePooler");

            InventoryItemPool = InventoryItemPooler.Pool;
            DamageNumberPool = DamageNumberPooler.Pool;
            // ProjectilePool = ProjectilePooler.Pool;
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

            if (Input.GetKeyDown(KeyCode.F))
            {
                InventoryController.ConsumeSelectedItem();
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

        public void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            PlayerConnected = true;
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
            int index = UnityEngine.Random.Range(0, PlayerSpawnLocations.Count);
            return PlayerSpawnLocations[index].position;
        }

        public Transform GetPlayerSpawn(int index)
        {
            return PlayerSpawnLocations[index];
        }

        internal InventoryItem FindInventoryItem(InventoryItemData data, bool createNew = true)
        {
            return InventoryController.FindInventoryItem(data, createNew);
        }

        public void InitializePlayers()
        {
            foreach(PlayerCharacter playerCharacter in PlayerCharacterList)
            {
                playerCharacter.InitializePlayerChar();
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

        // ## ObjectPooling ##

        public InventoryItem GetInventoryItem()
        {
            return InventoryItemPool.Get();
        }

        public void ReleaseInventoryItem(InventoryItem pooledObject)
        {
            InventoryItemPool.Release(pooledObject);
        }

        public DamageNumber GetDamageNumber(Vector3 position, int damage, float disappearTimer = 2f, float fadeOutSpeed = 1f, float moveUpSpeed = 1f)
        {
            return DamageNumberPool.Get().Initialize(position, damage, disappearTimer, fadeOutSpeed, moveUpSpeed);
        }

        public void ReleaseDamageNumber(DamageNumber pooledObject)
        {
            DamageNumberPool.Release(pooledObject);
        }
        // public Projectile GetProjectile()
        // {
        //     return ProjectilePool.Get();
        // }

        // public void ReleaseProjectile(Projectile pooledObject)
        // {
        //     ProjectilePool.Release(pooledObject);
        // }
    }
}

