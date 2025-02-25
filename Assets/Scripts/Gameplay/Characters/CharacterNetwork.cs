using UnityEngine;
using Unity.Netcode;
using System;
using Blessing.Gameplay.Characters.Traits;
using System.Collections.Generic;

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

        public NetworkList<TraitData> TraitDataNetworkList = new NetworkList<TraitData>
        (
            new List<TraitData>(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public List<TraitData> TraitDataLocalList = new();
        
        public Vector2Int ComboMoveIndex { get { return comboMoveIndex.Value; } }

        public CharacterStateMachine CharacterStateMachine { get; private set; }
        // private NetworkObject networkObject;
        
        void Awake()
        {
            // networkObject = GetComponent<NetworkObject>();
            if (ShowDebug) Debug.Log(gameObject.name + ": Awake - networkObject - " + NetworkObject);

            // TraitDataNetworkList = new NetworkList<TraitData>
            // (
            //     new List<TraitData>(),
            //     NetworkVariableReadPermission.Everyone,
            //     NetworkVariableWritePermission.Owner
            // );
        }

        public override void OnNetworkSpawn()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkSpawn");

            base.OnNetworkSpawn();
            Character = GetComponent<Character>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();

            if (HasAuthority)
                stateIndex.Value = 0;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            stateIndex.OnValueChanged += OnNetworkStateIndexChanged;
            comboMoveIndex.OnValueChanged += OnNetworkComboMoveIndexChanged;
            TraitDataNetworkList.OnListChanged += OnTraitDataNetworkListChanged;

            if (GameManager.Singleton.PlayerConnected)
                Character.Initialize();
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                Character.Initialize();
            }
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

            // CharacterStateMachine.ComboIndex = newValue.x;
            // CharacterStateMachine.MoveIndex = newValue.y;
            CharacterStateMachine.ComboMoveIndex = newValue;
            CharacterStateMachine.UpdateCurrentMove();
        }
        protected void OnTraitDataNetworkListChanged(NetworkListEvent<TraitData> changeEvent)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnTraitDataNetworkListChanged");
            // Link com mais informações em como usar o NetworkListEvent 
            // https://discussions.unity.com/t/how-to-use-networklist/947471/2\

            // if (!UpdateLocalList(ref InventoryLocalList, InventoryNetworkList)) return;

            // TODO: fazer uma lógica que funciona caso mais de um item tenha mudado

            // Bug Está adicionando traits, mas não está tirando

            if (HasAuthority) return;

            if (!UpdateLocalList(ref TraitDataLocalList, TraitDataNetworkList)) return;
            
            Debug.Log(gameObject.name + ": UpdateLocalList Passou");

            Debug.Log(gameObject.name + ": changeEvent.Type - " + changeEvent.Type);

            if (changeEvent.Type == NetworkListEvent<TraitData>.EventType.Add)
            {
                Debug.Log(gameObject.name + ": Add Passou");
                Character.AddTrait(changeEvent.Value);
            }

            if (changeEvent.Type == NetworkListEvent<TraitData>.EventType.Remove)
            {
                Debug.Log(gameObject.name + ": Remove Passou");
                Character.RemoveTrait(changeEvent.Value);
            }
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) // Mover para PlayerCharacterNetwork
        {
            base.OnOwnershipChanged(previous, current);
        }
        protected bool UpdateLocalList(ref List<TraitData> localList, NetworkList<TraitData> networkList)
        {
            return GameManager.Singleton.UpdateLocalList(ref localList, networkList);
        }

        public void SetStateIndex(int index) // mover para CharacterNetwork
        {
            if (HasAuthority)
                stateIndex.Value = index;
        }

        internal void SetComboMoveIndex(Vector2Int comboMoveIndex)
        {
            if (HasAuthority)
            {
                this.comboMoveIndex.Value = comboMoveIndex;
            }
        }

        public virtual void GetOwnership()
        {
            GameManager.Singleton.GetOwnership(NetworkObject);
        }

        // Character RPC`s

        [Rpc(SendTo.Everyone)]
        public void ClearTargetListRpc()
        {
            Character.TargetList.Clear();
        }
    }
}

