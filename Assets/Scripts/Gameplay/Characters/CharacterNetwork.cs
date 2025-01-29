using UnityEngine;
using Unity.Netcode;
using System;

namespace Blessing.Gameplay.Characters
{
    public class CharacterNetwork : NetworkBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public Character Character { get; private set; }

        [field: SerializeField]
        protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public int StateIndex { get { return stateIndex.Value; } }

        public NetworkVariable<Vector2Int> comboMoveIndex = new NetworkVariable<Vector2Int>
            (
                new Vector2Int(0, 0),
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner
            );
        public Vector2Int ComboMoveIndex { get { return comboMoveIndex.Value; } }

        public CharacterStateMachine CharacterStateMachine { get; private set; }
        // private NetworkObject networkObject;
        
        void Awake()
        {
            // networkObject = GetComponent<NetworkObject>();
            if (ShowDebug) Debug.Log(gameObject.name + ": Awake - networkObject - " + NetworkObject);
        }

        public override void OnNetworkSpawn()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkSpawn");

            base.OnNetworkSpawn();
            Character = GetComponent<Character>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();

            if (HasAuthority)
                stateIndex.Value = 0;

            stateIndex.OnValueChanged += OnNetworkStateIndexChanged;
            comboMoveIndex.OnValueChanged += OnNetworkComboMoveIndexChanged;
        }

        protected virtual void OnNetworkStateIndexChanged(int previousValue, int newValue)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkStateIndexChanged");

            CharacterStateMachine.SetNextStateByIndex(stateIndex.Value);
        }

        protected virtual void OnNetworkComboMoveIndexChanged(Vector2Int previousValue, Vector2Int newValue)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkComboMoveIndexChanged");

            if (HasAuthority) return;

            CharacterStateMachine.ComboIndex = newValue.x;
            CharacterStateMachine.MoveIndex = newValue.y;
        }

        public void SetStateIndex(int index) // mover para CharacterNetwork
        {
            if (HasAuthority)
                stateIndex.Value = index;
        }

        internal void SetComboMoveIndex(int comboIndex, int moveIndex)
        {
            if (HasAuthority)
            {
                comboMoveIndex.Value = new Vector2Int(comboIndex, moveIndex);
            }
        }

        // [Rpc(SendTo.Everyone)]
        // public void ClearTargetListRpc() // mover para CharacterNetwork, não usar RPC
        // {
        //     Character.ClearTargetList();
        // }

        public virtual void GetOwnership()
        {
            GameManager.Singleton.GetOwnership(NetworkObject);
        }
    }
}

