using UnityEngine;
using Unity.Netcode;
using System;
using Blessing.Gameplay.Characters.Traits;
using System.Collections.Generic;
using UnityEngine.VFX;
using Unity.Collections;

namespace Blessing.Gameplay.Characters
{
    public class CharacterNetwork : NetworkBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public Character Character { get; private set; }
        public NetworkVariable<FixedString32Bytes> CharacterName = new();
        [field: SerializeField] protected NetworkVariable<bool> isTraveling = new NetworkVariable<bool>(false);
        public bool IsTraveling { get { return isTraveling.Value; } set { isTraveling.Value = value; } }

        [field: SerializeField]
        protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(0,
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
        public bool HasSpawned = false;

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

            HasSpawned = true;

            if (GameManager.Singleton.PlayerConnected)
                Character.Initialize();
        }
        public override void OnNetworkDespawn()
        {
            Debug.Log(gameObject.name + ": OnNetworkDespawn");

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            stateIndex.OnValueChanged -= OnNetworkStateIndexChanged;
            comboMoveIndex.OnValueChanged -= OnNetworkComboMoveIndexChanged;
            TraitDataNetworkList.OnListChanged -= OnTraitDataNetworkListChanged;
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            Debug.Log(gameObject.name + ": OnClientConnectedCallback");
            if (NetworkManager.Singleton.LocalClientId == clientId && GameManager.Singleton.PlayerConnected)
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

            // When change Ownership, if character is dead, call OnDeath on the new owner client
            if (Character.Health.IsDead)
            {
                Character.OnDeath();
            }
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

        [Rpc(SendTo.Everyone)]
        public void HandleTraitVisualEffectRpc(int traitId)
        {
            Trait trait = Character.GetTrait(traitId);

            if (trait == null) return;

            Vector3 targetLocation = transform.position;
            if (trait.SpawnVFXOnGround)
            {
                /*
            * Create the hit object
            * This will later hold the data for the hit
            * (location, collided collider etc.)
            */
                RaycastHit hit;

                /*
                 * The ray length.
                 * Modify it to change the length of the Ray.
                 */
                float distance = 100f;

                /*
                 * A variable to store the location of the hit.
                 */


                /*
                 * Cast a raycast.
                 * If it hits something:
                 */
                if (Physics.Raycast(transform.position, Vector3.down, out hit, distance))
                {
                    /*
                     * Get the location of the hit.
                     * This data can be modified and used to move your object.
                     */
                    targetLocation = hit.point;
                }
            }

            VisualEffect visualEffect = Instantiate(trait.VisualEffect, targetLocation + new Vector3(0, 0.05f, 0), Quaternion.identity);
            
            if (trait.VFXFollowChar)
                visualEffect.transform.SetParent(transform, true);

            visualEffect.Play();

            Destroy(visualEffect.gameObject, visualEffect.GetFloat("LifeTime"));
        }
    }
}

