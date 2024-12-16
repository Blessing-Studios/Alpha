using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

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
        }

        public void SetOwnerName(string name) // Mover para PlayerCharacterNetwork
        {
            OwnerName.Value = new FixedString32Bytes(name);
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

