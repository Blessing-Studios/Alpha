using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.HealthAndDamage;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Services.Vivox;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CharacterHealth))]
    public abstract class Character : NetworkBehaviour, IHitter, IHittable
    {
        public float AttackPressedTimerWindow = 0.2f;
        public MovementController MovementController { get; protected set; }
        public CharacterStateMachine CharacterStateMachine { get; protected set; }
        public CharacterHealth Health { get; protected set; }
        public HitInfo HitInfo { get; protected set; }
        [field: SerializeField] protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public int StateIndex { get { return stateIndex.Value;}}

        protected List<IHittable> targetList = new();
        protected virtual void Awake()
        {
            MovementController = GetComponent<MovementController>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();
            Health = GetComponent<CharacterHealth>();

            stateIndex.Value = 0;
        }

        public void SetStateIndex(int index)
        {
            if (HasAuthority)
                stateIndex.Value = index;
        }

        public override void OnNetworkSpawn()
        {
            stateIndex.OnValueChanged += OnNetworkStateIndexChanged;
        }

        protected virtual void OnNetworkStateIndexChanged(int previousValue, int newValue)
        {
            CharacterStateMachine.SetNextStateByIndex(stateIndex.Value);
        }

        public void ClearTargetList()
        {
            if (HasAuthority)
                ClearTargetListRpc();
        }

        [Rpc(SendTo.Everyone)]
        public void ClearTargetListRpc()
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