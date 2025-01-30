using System;
using System.Collections.Generic;
using Blessing.Characters;
using Blessing.Core.GameEventSystem;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.Characters
{
    public class CharacterGear : NetworkBehaviour, IInteractable
    {
        [Header("Character")]
        protected Character character;
        [SerializeField] protected int gold;
        public int Gold { get { return gold; } }
        //Checar se Gold é positivo

        public List<CharacterEquipment> Equipments;
        public NetworkList<InventoryItemData> EquipmentNetworkList;

        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)]
        public ScriptableObjectReference BackpackSlotType;
        [SerializeField] public EquipmentType BackpackSlot { get { return BackpackSlotType.value as EquipmentType; } }
        public Inventory Inventory;
        public List<InventoryItemData> EquipmentLocalList;
        private bool isEquipmentsInitialized = false;
        private Character looter;
        public bool CanInteract { get { return !character.Health.IsAlive; } }

        [Header("Events")]
        public GameEvent OnAddEquipment;
        public GameEvent OnRemoveEquipment;
        public CharacterStats GetStats()
        {
            return character.CharacterStats;
        }
        protected void Awake()
        {
            character = GetComponent<Character>();

            EquipmentNetworkList = new NetworkList<InventoryItemData>
                (
                    new List<InventoryItemData>(),
                    NetworkVariableReadPermission.Everyone,
                    NetworkVariableWritePermission.Owner
                );
        }

        protected virtual void Start()
        {
            if (BackpackSlot == null)
            {
                Debug.LogError(gameObject.name + ": BackpackSlot can't be null");
            }

            if (!HasAuthority)
                Initialize();
        }

        protected virtual void Update()
        {
            if (CanInteract)
                HandleStopInteraction();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EquipmentNetworkList.OnListChanged += OnEquipmentNetworkListChanged;

            // if (!HasAuthority)
            //     Initialize();
        }

        public void Initialize()
        {
            UpdateLocalList(ref EquipmentLocalList, EquipmentNetworkList);

            if (!isEquipmentsInitialized)
            {
                InitializeEquipments();
            }
        }

        private void InitializeEquipments()
        {
            RemoveAllEquipments();

            foreach (InventoryItemData itemData in EquipmentNetworkList)
            {
                InventoryItem inventoryItem = FindItem(itemData);
                AddEquipment(inventoryItem);

                Backpack backpack = inventoryItem.Item as Backpack;

                if (backpack != null)
                {
                    SetInventory(inventoryItem);
                }
            }

            isEquipmentsInitialized = true;
        }

        private void OnEquipmentNetworkListChanged(NetworkListEvent<InventoryItemData> changeEvent)
        {
            if (!UpdateLocalList(ref EquipmentLocalList, EquipmentNetworkList)) return;

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Add)
            {
                InventoryItem item = FindItem(changeEvent.Value);
                AddEquipment(item);
            }

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Remove)
            {
                RemoveEquipment(changeEvent.Value);
            }
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0) return false;

            if (gold < amount) return false;

            gold -= amount;
            return true;
        }

        public bool GainGold(int amount)
        {
            if (amount < 0) return false;

            gold += amount;
            return true;
        }
        public bool AddEquipment(InventoryItem inventoryItem)
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                if (AddEquipment(equipment, inventoryItem)) return true;
            }

            return false;
        }
        public bool AddEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            Gear gear = inventoryItem.Item as Gear;
            if (gear == null)
            {
                return false;
            }
            if (equipment.GearSlotType == gear.GearType)
            {

                if (!equipment.SetEquipment(inventoryItem)) return false;

                ApplyEquipmentTraits(equipment);

                inventoryItem.Data.Position = Vector2Int.zero;

                // Raise Events
                if (OnAddEquipment != null)
                    OnAddEquipment.Raise(this, equipment);

                if (HasAuthority)
                {
                    EquipmentLocalList.Add(inventoryItem.Data);
                    EquipmentNetworkList.Add(inventoryItem.Data);
                }

                return true;
            }

            return false;
        }

        private void RemoveAllEquipments()
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                RemoveEquipmentTraits(equipment);
                equipment.Unequip();
            }
        }

        public bool RemoveEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            Gear gear = inventoryItem.Item as Gear;
            if (equipment.GearSlotType == gear.GearType)
            {
                if (HasAuthority)
                {
                    EquipmentLocalList.Remove(inventoryItem.Data);
                    EquipmentNetworkList.Remove(inventoryItem.Data);
                }

                RemoveEquipmentTraits(equipment);

                equipment.Unequip();

                // Raise Events
                if (OnRemoveEquipment != null)
                    OnRemoveEquipment.Raise(this, equipment);

                return true;
            }

            return false;
        }

        public bool RemoveEquipment(InventoryItemData data)
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                if (equipment.InventoryItem == null) continue;

                if (equipment.InventoryItem.Data.Id == data.Id)
                {
                    if (HasAuthority)
                    {
                        EquipmentLocalList.Remove(data);
                        EquipmentNetworkList.Remove(data);
                    }

                    RemoveEquipmentTraits(equipment);

                    equipment.Unequip();

                    if (OnRemoveEquipment != null)
                        OnRemoveEquipment.Raise(this, equipment);
                }
            }

            return false;
        }

        public void ApplyEquipmentTraits(CharacterEquipment equipment)
        {
            if (equipment.InventoryItem == null) return;

            Gear gear = equipment.InventoryItem.Item as Gear;
            foreach (Trait trait in gear.Traits)
            {
                character.CharacterStats.AddTrait(trait);
            }
        }

        public void RemoveEquipmentTraits(CharacterEquipment equipment)
        {
            if (equipment.InventoryItem == null) return;

            Gear gear = equipment.InventoryItem.Item as Gear;
            foreach (Trait trait in gear.Traits)
            {
                character.CharacterStats.RemoveTrait(trait);
            }
        }

        public void ApplyAllEquipmentsTraits()
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                ApplyEquipmentTraits(equipment);
            }
        }

        public void ValidateEquipments()
        {
            // TODO:
            // CharactersEquipments can't repeat slots
        }

        protected InventoryItem CreateItem(InventoryItemData data)
        {
            return GameManager.Singleton.InventoryController.CreateItem(data);
        }

        protected InventoryItem FindItem(InventoryItemData data)
        {
            return GameManager.Singleton.FindInventoryItem(data);
        }

        protected bool UpdateLocalList(ref List<InventoryItemData> localList, NetworkList<InventoryItemData> networkList)
        {
            return GameManager.Singleton.UpdateLocalList(ref localList, networkList);
        }
        public virtual void AddBackpack(InventoryItem inventoryItem)
        {
            if (inventoryItem != null)
            {
                SetInventory(inventoryItem);
            }
        }
        public virtual void RemoveBackpack()
        {
            UnequipInventory();
        }

        protected virtual void SetInventory(InventoryItem inventoryItem)
        {
            if (inventoryItem.Inventory == null)
            {
                Debug.Log(gameObject.name + ": inventoryItem.Inventory == null - " + inventoryItem.Item.name);
                return;
            }

            Debug.Log(gameObject.name + ": inventoryItem.Inventory != null - " + inventoryItem.Item.name);

            Inventory = inventoryItem.Inventory;
        }

        protected void UnequipInventory()
        {
            Inventory = null;
        }
        public Vector2Int GetWeaponDamageAndPen()
        {
            // Calculate weapon damage
            // Find Weapon to get base attack

            Weapon weapon = GetEquippedWeapon();
            if (weapon == null) return new Vector2Int(character.CharacterStats.Strength * 10, 0);

            // Calculate Damage

            int damage = weapon.Attack;

            foreach(WeaponModifier modifier in weapon.WeaponModifiers)
            {
                damage += character.CharacterStats.GetStatValue(modifier.Stat) * modifier.Value;
            }

            return new Vector2Int(damage, weapon.DamageClass);
        }

        public Weapon GetEquippedWeapon()
        {
            foreach(CharacterEquipment equipment in Equipments)
            {
                if (equipment.InventoryItem == null) continue;

                Weapon weapon = equipment.InventoryItem.Item as Weapon;
                if (weapon != null)
                {
                    return weapon;
                }
            }

            return null;
        }

        public void Interact(Interactor interactor)
        {
            Debug.Log(gameObject.name + ": CharacterGear Entrou Interact");
            if (!CanInteract) return;

            if (interactor.gameObject.TryGetComponent(out Character looter))
            {
                // Set Grids
                GameManager.Singleton.GetOwnership(NetworkObject);

                this.looter = looter;

                GameManager.Singleton.InventoryController.OtherCharacter = character;

                if (!GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    GameManager.Singleton.InventoryController.OpenAllGrids();
                }
                else if (GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    Debug.Log(gameObject.name + ": CharacterGear Interact IsGridsOpen = true");
                    GameManager.Singleton.InventoryController.CloseAllGrids();
                }
            }
        }
        private void HandleStopInteraction()
        {
            if (looter == null) return;

            float maxDistance = (float)(looter.CharacterStats.Dexterity + looter.CharacterStats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, looter.transform.position);

            if (distance > maxDistance)
            {
                looter = null;
                GameManager.Singleton.InventoryController.CloseAllGrids();
            }
        }
    }
}

