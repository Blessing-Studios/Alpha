using UnityEngine;
using Blessing.Core;
using Unity.Netcode;
using Blessing.GameData;
using Unity.Collections;

namespace Blessing.Player
{
    public class PlayerCharacter : NetworkBehaviour
    {
        public NetworkVariable<FixedString32Bytes> OwnerName = new();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public string GetOwnerName()
        {
            return OwnerName.Value.ToString();
        }
        void Start()
        {
            if (HasAuthority)
            {
                OwnerName.Value = new FixedString32Bytes(GameDataManager.Singleton.PlayerName);
            }

            GameManager.Singleton.VirtualCamera.LookAt = transform;
            GameManager.Singleton.VirtualCamera.Target.TrackingTarget = transform;
        }
    }
}