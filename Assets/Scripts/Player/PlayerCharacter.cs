using Blessing.Gameplay.Characters;
using Blessing.GameData;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace Blessing.Player
{
    [RequireComponent(typeof(PlayerMovementController))]
    public class PlayerCharacter : Character
    {
        [HideInInspector] public NetworkVariable<FixedString32Bytes> OwnerName = new();
        public NetworkVariable<bool> IsDisabled = new();
        // private int deferredDespawnTicks = 4;
        public string GetOwnerName()
        {
            return OwnerName.Value.ToString();
        }

        void Start()
        {
            HandleInitialization();
        }

        public void Update()
        {
            if (IsDisabled.Value == true && gameObject.activeSelf == true)
            {
                gameObject.SetActive(false);
                // NetworkObject.DeferDespawn(deferredDespawnTicks, destroy: true);
            }   
        }

        private void HandleInitialization()
        {
            if (HasAuthority)
            {
                IsDisabled.Value = false;
                OwnerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);
                
                if (GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
                {
                    Debug.LogWarning("Esse player já tem um Char");

                    PlayerCharacter originalCharacter = GameManager.Singleton.PlayerCharactersDic[GetOwnerName()];

                    originalCharacter.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

                    GameManager.Singleton.VirtualCamera.LookAt = originalCharacter.transform;
                    GameManager.Singleton.VirtualCamera.Target.TrackingTarget = originalCharacter.transform;

                    IsDisabled.Value = true;
                }
                else // This player doesn't have a Char in this session, use this char
                {
                    GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
                    GameManager.Singleton.PlayerCharacterList.Add(this);

                    GameManager.Singleton.VirtualCamera.LookAt = transform;
                    GameManager.Singleton.VirtualCamera.Target.TrackingTarget = transform;
                }
            }

            if (!HasAuthority && !GameManager.Singleton.PlayerCharactersDic.ContainsKey(GetOwnerName()))
            {
                GameManager.Singleton.AddPlayerCharacter(GetOwnerName(), this);
                GameManager.Singleton.PlayerCharacterList.Add(this);
            }

            gameObject.name = gameObject.name + "-" + GetOwnerName();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            if (!HasAuthority) return;

            if (GameDataManager.Singleton.PlayerName == GetOwnerName())
                GetComponent<NetworkObject>().SetOwnershipLock(true);
            else 
                GetComponent<NetworkObject>().SetOwnershipLock(false);
            
        }

        public override bool CheckIfAttackPressed(string nextComboAction)
        {
            // Fazer essa lógica
            return false;
        }
    }
}