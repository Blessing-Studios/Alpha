using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;
using TMPro;
using System.Collections;

namespace Blessing.Player
{
    public class PlayerCharacterNetwork : CharacterNetwork
    {
        public NetworkVariable<FixedString32Bytes> OwnerName = new();
        public NetworkVariable<bool> IsDisabled = new();
        protected NetworkVariable<int> m_TickToChangeOwnership = new NetworkVariable<int>();
        public int ChangeOwnershipTime = 2;
        public PlayerCharacter PlayerCharacter { get; private set; }
        public TextMeshPro GuidText;
        void Awake()
        {
            PlayerCharacter = GetComponent<PlayerCharacter>();

            OwnerName.OnValueChanged += OnOwnerNameValueChanged;

            m_TickToChangeOwnership.Value = ChangeOwnershipTime;
        }

        void Update()
        {
            if (ShowDebug)
            {
                GuidText.gameObject.SetActive(true);
                GuidText.text = NetworkObject.OwnerClientId.ToString(); ;
            }
            else
                GuidText.gameObject.SetActive(false);
        }

        private void OnOwnerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            // Will set the values on the Clients that don't Have Authority
            if (HasAuthority) return;

            PlayerCharacter.InitializePlayerChar();

            // string ownerName = newValue.ToString();

            // gameObject.name = "Char-" + ownerName;

            // if (!GameManager.Singleton.PlayerCharactersDic.ContainsKey(ownerName))
            // {
            //     GameManager.Singleton.AddPlayerCharacter(ownerName, PlayerCharacter);
            //     GameManager.Singleton.PlayerCharacterList.Add(PlayerCharacter);

            //     // Find Player Controller
            //     foreach (PlayerController player in GameManager.Singleton.PlayerList)
            //     {
            //         if (player.GetPlayerName() == ownerName)
            //         {
            //             player.SetPlayerCharacter(PlayerCharacter);
            //             return;
            //         }
            //     }
            // }
        }

        public void SetPlayerOwnerName(string name) // Mover para PlayerCharacterNetwork
        {
            if (!HasAuthority) return;

            OwnerName.Value = new FixedString32Bytes(name);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // if (HasAuthority) return;

            // PlayerCharacter.SetCanGiveInputs(GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()));

            // if (!PlayerCharacter.CanGiveInputs && !GameManager.Singleton.PlayerCharactersDic.ContainsKey(OwnerName.Value.ToString()))
            // {
            //     GameManager.Singleton.AddPlayerCharacter(OwnerName.Value.ToString(), PlayerCharacter);
            //     GameManager.Singleton.PlayerCharacterList.Add(PlayerCharacter);

            //     gameObject.name = "Char-" + OwnerName.Value.ToString();
            //     Debug.Log(gameObject.name + " OnNetworkSpawn()");

            //     // Find Player Controller
            //     // foreach (PlayerController player in GameManager.Singleton.PlayerList)
            //     // {
            //     //     if (player.GetPlayerName() == GetPlayerOwnerName())
            //     //     {
            //     //         player.SetPlayerCharacter(this);
            //     //         return;
            //     //     }
            //     // }

            // }
        }

        public override void GetOwnership()
        {
            // if (hasOnNetworkSpawnRan)
            // {
            //     GameManager.Singleton.GetOwnership(NetworkObject);
            // }
            // else
            // {
            //     delayGetOwnerShip = true;
            // }  
            if (GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()))
                StartCoroutine(WaitToGetOwnership());

        }

        IEnumerator WaitToGetOwnership()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkTickSystem.ServerTime.Tick > m_TickToChangeOwnership.Value);

            GameManager.Singleton.GetOwnership(NetworkObject);

            StopCoroutine(WaitToGetOwnership());
        }

        public override void OnNetworkDespawn()
        {
            StopAllCoroutines();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            base.OnOwnershipChanged(previous, current);
            // if (!HasAuthority) return;

            // if (GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()))
            //     GetComponent<NetworkObject>().SetOwnershipLock(true);
            // else
            //     GetComponent<NetworkObject>().SetOwnershipLock(false);

            PlayerCharacter.SetCanGiveInputs(GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()));
        }
    }
}

