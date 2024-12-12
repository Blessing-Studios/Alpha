using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.HealthAndDamage;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Netcode.Components;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(CharacterController))]
    public abstract class Character : NetworkBehaviour, IHitter, IHittable
    {
        protected string characterName;
        public string CharacterName
        {
            get => characterName;
            protected set => characterName = value;
        }
        
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public float AttackPressedTimerWindow = 0.2f;
        public MovementController MovementController { get; protected set; }
        public CharacterStateMachine CharacterStateMachine { get; protected set; }
        public CharacterHealth Health { get; protected set; }
        public CharacterController CharacterController { get; protected set; }
        public HitInfo HitInfo { get; protected set; }
        [field: SerializeField] protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public int StateIndex { get { return stateIndex.Value; }}
        protected List<IHittable> targetList = new();
        public Vector3 SpawnLocation;
        protected virtual void Awake()
        {
            MovementController = GetComponent<MovementController>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();
            Health = GetComponent<CharacterHealth>();
            CharacterController = GetComponent<CharacterController>();

            stateIndex.Value = 0;
        }

        protected virtual void Start() 
        {
            if (HasAuthority)
            {
                CharacterController.enabled = false;
                CharacterController.transform.position = SpawnLocation;
                CharacterController.enabled = true;
            }
        }

        public void SetStateIndex(int index) // mover para CharacterNetwork
        {
            if (HasAuthority)
                stateIndex.Value = index;
        }

        public override void OnNetworkSpawn() // mover para CharacterNetwork
        {
            base.OnNetworkSpawn();
            if (ShowDebug) Debug.Log(gameObject.name + " OnNetworkSpawn");
            stateIndex.OnValueChanged += OnNetworkStateIndexChanged;
        }

        protected virtual void OnNetworkStateIndexChanged(int previousValue, int newValue) // mover para CharacterNetwork
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": OnNetworkStateIndexChanged");
            CharacterStateMachine.SetNextStateByIndex(stateIndex.Value);
        }

        public void ClearTargetList()
        {
            if (HasAuthority)
                ClearTargetListRpc();
        }

        [Rpc(SendTo.Everyone)]
        public void ClearTargetListRpc() // mover para CharacterNetwork, não usar RPC
        {
            targetList.Clear();
        }

        /// <summary>
        /// Add target to target list
        /// </summary>
        /// <param name="target"></param>
        /// <returns>false if failed already hit, true if added to targetList</returns>
        public virtual bool Hit(IHittable target)
        {
            if (targetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            HitInfo = new HitInfo(CharacterStateMachine.CurrentMove.Damage);
            targetList.Add(target);

            return true;
        }

        public virtual void GotHit(IHitter hitter)
        {
            if (!HasAuthority) return;

            //Receive Damage
            Health.ReceiveDamage(hitter.HitInfo.Damage);

            int health = Health.CurrentHealth;
            if (health > 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.TakeHitState);

            if (health <= 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.DeadState);
        }
        public abstract bool CheckIfActionTriggered(string actionName);
    }
}