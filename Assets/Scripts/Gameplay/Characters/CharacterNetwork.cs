using UnityEngine;
using Unity.Netcode;

namespace Blessing.Gameplay.Characters
{
    public class CharacterNetwork : NetworkBehaviour
    {
        [field: SerializeField] public Character Character { get; private set; }
        [field: SerializeField] public bool ShowDebug { get; private set; }

        [field: SerializeField]
        protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public int StateIndex { get { return stateIndex.Value; } }

        public CharacterStateMachine CharacterStateMachine { get; private set; }
        
        void Awake()
        {
            CharacterStateMachine = Character.GetComponent<CharacterStateMachine>();
        }

        void Start()
        {

        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (ShowDebug) Debug.Log(gameObject.name + " OnNetworkSpawn");
            stateIndex.OnValueChanged += OnNetworkStateIndexChanged;
        }

        protected virtual void OnNetworkStateIndexChanged(int previousValue, int newValue)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkStateIndexChanged");
            CharacterStateMachine.SetNextStateByIndex(stateIndex.Value);
        }

        public void SetStateIndex(int index) // mover para CharacterNetwork
        {
            if (HasAuthority)
                stateIndex.Value = index;
        }

        [Rpc(SendTo.Everyone)]
        public void ClearTargetListRpc() // mover para CharacterNetwork, não usar RPC
        {
            targetList.Clear();
        }
    }
}

