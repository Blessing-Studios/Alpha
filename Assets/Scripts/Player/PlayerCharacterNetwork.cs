using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System;

namespace Blessing.Player
{
    public class PlayerCharacterNetwork : CharacterNetwork
    {
        public NetworkVariable<FixedString32Bytes> OwnerName = new();
        public NetworkVariable<bool> IsDisabled = new();
        public PlayerCharacter PlayerCharacter { get; private set; }

        void Awake()
        {
            PlayerCharacter = GetComponent<PlayerCharacter>();

            OwnerName.OnValueChanged += OnOwnerNameValueChanged;
        }

        private void OnOwnerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            // Will set the values on the Clients that don't Have Authority
            if (HasAuthority) return;

            string ownerName = newValue.ToString();

            gameObject.name = "Char-" + ownerName;

            if (!GameManager.Singleton.PlayerCharactersDic.ContainsKey(ownerName))
            {
                GameManager.Singleton.AddPlayerCharacter(ownerName, PlayerCharacter);
                GameManager.Singleton.PlayerCharacterList.Add(PlayerCharacter);

                // Find Player Controller
                foreach (PlayerController player in GameManager.Singleton.PlayerList)
                {
                    if (player.GetPlayerName() == ownerName)
                    {
                        player.SetPlayerCharacter(PlayerCharacter);
                        return;
                    }
                }
            }
        }

        public void SetOwnerName(string name) // Mover para PlayerCharacterNetwork
        {
            OwnerName.Value = new FixedString32Bytes(name);
        }



        public override void OnNetworkSpawn()
        {
            if (!HasAuthority)
                Debug.Log(gameObject.name + ": OwnerName() - " + OwnerName.Value);
        } 

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            if (!HasAuthority) return;
            
            if (GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()))
                GetComponent<NetworkObject>().SetOwnershipLock(true);
            else
                GetComponent<NetworkObject>().SetOwnershipLock(false);
            

            PlayerCharacter.SetCanGiveInputs(GameDataManager.Singleton.ValidateOwner(OwnerName.Value.ToString()));
        }
    }
}

