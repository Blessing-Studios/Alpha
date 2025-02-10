using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.HealthAndDamage;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Netcode.Components;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.SkillsAndMagic;
using System.Collections;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterGear))]
    [RequireComponent(typeof(CharacterMana))]
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
        public CharacterMana Mana { get; protected set; }
        public CharacterStats Stats { get; protected set; }
        public CharacterController CharacterController { get; protected set; }
        public CharacterNetwork CharacterNetwork { get; protected set; }
        [field: SerializeField] protected InputActionList actionList;
        [field: SerializeField] protected InputDirectionList directionList;
        public InputActionType TriggerAction { get; protected set; }
        public InputDirectionType TriggerDirection { get; protected set; }
        protected Dictionary<string, InputActionType> inputActionsDic = new();
        protected Dictionary<string, InputDirectionType> inputDirectionsDic = new();
        [field: SerializeField] public Vector2Int DamageAndPen { get; protected set; }
        [field: SerializeField] public Vector2Int DefenseAndPenRes { get; protected set; }
        public List<Trait> Traits;
        [field: SerializeField] public List<IHittable> TargetList { get; private set; }

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

        public void SetComboMoveIndex(int comboIndex, int moveIndex)
        {
            if (CharacterNetwork != null)
                CharacterNetwork.SetComboMoveIndex(comboIndex, moveIndex);
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
            Mana = GetComponent<CharacterMana>();
            Stats = GetComponent<CharacterStats>();

            CharacterController = GetComponent<CharacterController>();

            CharacterNetwork = GetComponent<CharacterNetwork>();

            TargetList = new List<IHittable>();

            foreach (InputActionType InputAction in actionList.InputActions)
            {
                inputActionsDic.Add(InputAction.Name, InputAction);
            }

            foreach (InputDirectionType InputDirection in directionList.InputDirections)
            {
                inputDirectionsDic.Add(InputDirection.Name, InputDirection);
            }

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

        public virtual void Initialize()
        {
            //
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

        public virtual bool Hit(IHittable target)
        {

            if (target as Character == this)
            {
                // Can't hit itself
                return false;
            }

            if (TargetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            // If CurrentMove is null, it will throw a error
            HitInfo = new HitInfo(DamageAndPen.x, DamageAndPen.y);
            TargetList.Add(target);

            return true;
        }

        public virtual void GotHit(IHitter hitter)
        {
            if (!HasAuthority) return;

            // Por enquanto, não pode bater em characters mortos
            if (Health.IsDead) return;

            // Apply Armour Damage Reduction

            // Subtrair Pen com PenRes
            int armorPen = hitter.HitInfo.DamageClass - DefenseAndPenRes.y;

            int damage = hitter.HitInfo.Damage;
            int defense = DefenseAndPenRes.x;

            if (armorPen < 0)
            {
                damage = (int) (damage * ( 1 - armorPen * 0.25f));
                damage = damage < 0 ? 0 : damage;
            }

            if (armorPen > 0)
            {
                defense = (int) (defense * ( 1 - armorPen * 0.25f));
                defense = defense < 0 ? 0 : defense;
            }

            int appliedDamage = damage - defense;
            appliedDamage = appliedDamage < 0 ? 0 : appliedDamage;

            Debug.Log(gameObject.name + ": appliedDamage - " + appliedDamage);

            //Receive Damage
            Health.ReceiveDamage(appliedDamage);

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
        public void GetOwnership()
        {
            CharacterNetwork.GetOwnership();
        }
        public InputActionType GetAction(string name)
        {
            return inputActionsDic[name];
        }

        public InputDirectionType GetDirection(string name)
        {
            return inputDirectionsDic[name];
        }
        public abstract bool CheckIfActionTriggered(string actionName);

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

        public bool AddTrait(Trait trait)
        {
            // Check if Trait can be added

            Traits.Add(trait);
            UpdateParameters();
            return true;
        }

        public bool RemoveTrait(Trait trait)
        {
            // Check if Trait can be removed
            
            Traits.Remove(trait);
            UpdateParameters();
            return true;
        }

        public void ApplyBuff(Buff buff)
        {
            StartCoroutine(HandleBuff(buff));
        }

        private IEnumerator HandleBuff(Buff buff)
        {
            AddTrait(buff);

            yield return new WaitForSeconds(buff.Duration);

            RemoveTrait(buff);
        }

        public void UpdateParameters()
        {
            // Update Stats
            Stats.UpdateAllStats(Traits);

            // Update Health
            Health.SetHealthParameters(Stats.Constitution, Traits);

            // Update Damage and Defense
            DamageAndPen = Gear.GetWeaponDamageAndPen(Traits);
            DefenseAndPenRes = Gear.GetArmorDefenseAndPen(Traits);

            // Update Mana
            Mana.SetManaParameters(Stats, Traits);
        }

        // GameEventListeners
        public void OnAddEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnAddEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            InventoryItem inventoryItem = characterEquipment.InventoryItem;

            Backpack backpack = inventoryItem.Item as Backpack;

            if (backpack != null)
            {
                Gear.AddBackpack(inventoryItem);
            }

            GameManager.Singleton.InventoryController.SyncGrids();
        }

        public void OnRemoveEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnRemoveEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            if (characterEquipment.GearSlotType == Gear.BackpackSlot)
            {
                Gear.RemoveBackpack();
            }

            GameManager.Singleton.InventoryController.SyncGrids();
        }
    }
}