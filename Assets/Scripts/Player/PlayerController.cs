using System;
using Blessing.GameData;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerController : NetworkBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public NetworkObject CharacterToSpawn;
        public Vector3 SpawnLocation;
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }
        [HideInInspector] public NetworkVariable<FixedString32Bytes> PlayerName = new();
        public string GetPlayerName()
        {
            return PlayerName.Value.ToString();
        }
        public NetworkVariable<bool> IsDisabled = new();
        // private int deferredDespawnTicks = 4;

        public override void OnNetworkSpawn()
        {
            PlayerName.OnValueChanged += OnNetworkPlayerNameChanged;
        }

        private void OnNetworkPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            gameObject.name = "Player-" + newValue.ToString();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameManager.Singleton.AddPlayer(this);

            if (GameManager.Singleton.SceneStarter.HasStarted.Value)
                Initialization();
        }

        public void Initialization()
        {
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
        }
        private void FindPlayerCharacter()
        {
            PlayerCharacter = GameManager.Singleton.PlayerCharactersDic[GetPlayerName()];
            PlayerCharacter.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);
        }

        private void SpawnPlayerCharacter()
        {
            SpawnLocation = GameManager.Singleton.GetPlayerSpawnPosition();

            if (SpawnLocation == null)
                Debug.LogError(gameObject.name + " SpawnLocation is missing.");

            var spawnedNetworkObject = CharacterToSpawn.InstantiateAndSpawn(NetworkManager, ownerClientId: OwnerClientId);

            spawnedNetworkObject.name = "Char-" + GetPlayerName();
            PlayerCharacter = spawnedNetworkObject.GetComponent<PlayerCharacter>();
            PlayerCharacter.SpawnLocation = SpawnLocation;
            PlayerCharacter.SetOwnerName(GetPlayerName());

            GameManager.Singleton.AddPlayerCharacter(GetPlayerName(), PlayerCharacter);
            GameManager.Singleton.PlayerCharacterList.Add(PlayerCharacter);
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
    }
}

