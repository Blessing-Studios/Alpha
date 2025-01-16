using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.HealthAndDamage;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Netcode.Components;
using Blessing.Characters;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Interation;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterGear))]
    [RequireComponent(typeof(CharacterStats))]
    public abstract class Character : MonoBehaviour, IHitter, IHittable
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
        public CharacterGear Gear { get; protected set; }
        public CharacterStats CharacterStats { get; protected set; }
        [field: SerializeField] public List<IHittable> TargetList { get; private set; }
        public CharacterController CharacterController { get; protected set; }
        public CharacterNetwork CharacterNetwork { get; protected set; }
        public HitInfo HitInfo { get; protected set; }
        // [field: SerializeField] protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
        //     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Mover para CharacterNetwork

        // public int StateIndex { get { return stateIndex.Value; }}

        public int StateIndex 
        { 
            get 
            {
                if (CharacterNetwork != null)
                    return CharacterNetwork.StateIndex;
                else
                    return stateIndex;
            }
        }
        public void SetStateIndex(int stateIndex) // Gambiarra para funcionar tanto online quanto offline
        {
            if (CharacterNetwork != null) 
                CharacterNetwork.SetStateIndex(stateIndex);
            else
                this.stateIndex = stateIndex;
        }

        private int stateIndex; // Offline StateIndex
        public Vector3 SpawnLocation;

        public bool HasAuthority 
        { 
            get 
            {
                if (CharacterNetwork != null)
                    return CharacterNetwork.HasAuthority;
                else
                    return true;
            } 
        }

        protected virtual void Awake()
        {
            MovementController = GetComponent<MovementController>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();
            Health = GetComponent<CharacterHealth>();
            Gear = GetComponent<CharacterGear>();
            CharacterStats = GetComponent<CharacterStats>();

            CharacterController = GetComponent<CharacterController>();

            CharacterNetwork = GetComponent<CharacterNetwork>();

            TargetList = new List<IHittable>();

            // stateIndex.Value = 0;
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

        public void ClearTargetList()
        {
            // if (!HasAuthority) return;
            
            TargetList.Clear();
        }

        // [Rpc(SendTo.Everyone)]
        // public void ClearTargetListRpc() // mover para CharacterNetwork, não usar RPC
        // {
        //     TargetList.Clear();
        // }

        /// <summary>
        /// Add target to target list
        /// </summary>
        /// <param name="target"></param>
        /// <returns>false if failed already hit, true if added to TargetList</returns>
        public virtual bool Hit(IHittable target)
        {

            if ((Character) target == this)
            {
                // Can't hit itself
                return false;
            }

            if (TargetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            HitInfo = new HitInfo(CharacterStateMachine.CurrentMove.Damage);
            TargetList.Add(target);

            return true;
        }

        public virtual void GotHit(IHitter hitter)
        {
            if (!HasAuthority) return;

            // Por enquanto, não pode bater em characters mortos
            if (Health.IsDead) return;

            //Receive Damage
            Health.ReceiveDamage(hitter.HitInfo.Damage);

            int health = Health.CurrentHealth;
            if (health > 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.TakeHitState);

            if (health <= 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.DeadState);
        }

        public virtual void OnDeath()
        {
            Health.SetCharacterAsDead();
            MovementController.DisableMovement();
            MovementController.DisableCollision();
        }

        // public void Interact(Interactor interactor)
        // {
        //     if (Health.IsAlive) return;

        //     if (interactor.gameObject.TryGetComponent(out Character looter))
        //     {
        //         this.looter = looter;

        //         Gear.Inventory.InventoryGrid.Inventory = Gear.Inventory;
        //         if (!Gear.Inventory.InventoryGrid.IsOpen)
        //         {
        //             Gear.Inventory.GetOwnership();
        //             Gear.Inventory.InventoryGrid.ToggleGrid();
        //         }
        //         else if (Gear.Inventory.InventoryGrid.IsOpen)
        //         {
        //             Gear.Inventory.InventoryGrid.ToggleGrid();
        //         }
        //     }
        // }
        // private void HandleStopInteraction()
        // {
        //     if (looter == null) return;

        //     if (!Gear.Inventory.InventoryGrid.IsOpen) return;

        //     float maxDistance = (float ) (looter.CharacterStats.Dexterity + looter.CharacterStats.Luck) / 3;

        //     float distance = Vector3.Distance(transform.position, looter.transform.position);

        //     if (distance > maxDistance)
        //     {
        //         looter = null;
        //         Gear.Inventory.InventoryGrid.CloseGrid();
        //     }
        // }

        public void OnAddEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            Debug.Log(gameObject.name + ": OnAddEquipment");

            CharacterStats.UpdateAllStats();

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            InventoryItem inventoryItem = characterEquipment.InventoryItem;

            Backpack backpack = inventoryItem.Item as Backpack;

            if (backpack != null)
            {
                AddBackpack(inventoryItem);
            }

            GameManager.Singleton.InventoryController.SyncGrids();
        }

        public void OnRemoveEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            Debug.Log(gameObject.name + ": OnRemoveEquipment");

            CharacterStats.UpdateAllStats();

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            if (characterEquipment.GearSlotType == Gear.BackpackSlot)
            {
                RemoveBackpack();
            }

            GameManager.Singleton.InventoryController.SyncGrids();
        }

        public virtual void AddBackpack(InventoryItem inventoryItem)
        {
            Debug.Log(gameObject.name + ": OnRemoveBackpack");
            if (inventoryItem != null)
            {
                Gear.SetInventory(inventoryItem);
            }

                
        }

        public virtual void RemoveBackpack()
        {
            Debug.Log(gameObject.name + ": OnRemoveBackpack");
            Gear.UnequipInventory();
        }
        public abstract bool CheckIfActionTriggered(string actionName);
    }
}