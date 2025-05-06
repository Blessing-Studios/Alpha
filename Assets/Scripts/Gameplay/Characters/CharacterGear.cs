using System;
using System.Collections.Generic;
using Blessing.Core.GameEventSystem;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

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
        [SerializeField] private ScriptableObjectReference backpackSlot;
        public EquipmentType BackpackSlot { get { return backpackSlot.value as EquipmentType; } set { backpackSlot.value = value; }}

        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)]
        [SerializeField] private ScriptableObjectReference utilitySlot;
        public EquipmentType UtilitySlot { get { return utilitySlot.value as EquipmentType; } set { utilitySlot.value = value; }}

        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)]
        [SerializeField] private ScriptableObjectReference bodyArmorSlot;
        public EquipmentType BodyArmorSlot { get { return bodyArmorSlot.value as EquipmentType; } set { bodyArmorSlot.value = value; }}

        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)]
        [SerializeField] private ScriptableObjectReference weaponSlot;
        [SerializeField] public EquipmentType WeaponSlot { get { return weaponSlot.value as EquipmentType; } set { weaponSlot.value = value; }}
        public Inventory Inventory;
        public List<Inventory> UtilityInventories = new();
        public List<InventoryItemData> EquipmentLocalList = new();
        private bool isEquipmentsInitialized = false;
        private Character looter;
        public bool CanInteract { get { return !character.Health.IsAlive; } }

        [Header("Events")]
        public GameEvent OnAddEquipment;
        public GameEvent OnRemoveEquipment;
        public CharacterStats GetStats()
        {
            return character.Stats;
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
            // Remove and Add all equipment to force Sync
            
            foreach (CharacterEquipment equipment in Equipments)
            {
                RemoveEquipmentTraits(equipment);
                equipment.Unequip();
            }

            foreach (InventoryItemData itemData in EquipmentNetworkList)
            {
                InventoryItem inventoryItem = FindItem(itemData);
                AddEquipment(inventoryItem);

                // Backpack backpack = inventoryItem.Item as Backpack;
                // if (backpack != null)
                // {
                //     AddBackpack(inventoryItem);
                // }

                // Utility utility = inventoryItem.Item as Utility;
                // if (utility != null)
                // {
                //     AddUtility(inventoryItem);
                // }
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
            if (equipment.GearSlotType == gear.EquipmentType)
            {

                if (!equipment.SetEquipment(inventoryItem)) return false;

                if (HasAuthority)
                    ApplyEquipmentTraits(equipment);

                inventoryItem.Data.Position = Vector2Int.zero;

                Backpack backpack = inventoryItem.Item as Backpack;
                if (backpack != null)
                {
                    AddBackpack(inventoryItem);
                }

                Utility utility = inventoryItem.Item as Utility;
                if (utility != null)
                {
                    AddUtility(inventoryItem, equipment.DuplicateIndex);
                }

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

        public bool RemoveEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            Gear gear = inventoryItem.Item as Gear;
            if (equipment.GearSlotType == gear.EquipmentType)
            {
                if (HasAuthority)
                {
                    EquipmentLocalList.Remove(inventoryItem.Data);
                    EquipmentNetworkList.Remove(inventoryItem.Data);
                }

                RemoveEquipmentTraits(equipment);

                equipment.Unequip();

                if (equipment.GearSlotType == BackpackSlot)
                {
                    RemoveBackpack();
                }

                if (equipment.GearSlotType == UtilitySlot)
                {
                    RemoveUtility(equipment.DuplicateIndex);
                }

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

                    if (equipment.GearSlotType == BackpackSlot)
                    {
                        RemoveBackpack();
                    }

                    if (equipment.GearSlotType == UtilitySlot)
                    {
                        RemoveUtility(equipment.DuplicateIndex);
                    }

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
                character.AddTrait(trait);
            }
        }

        public void RemoveEquipmentTraits(CharacterEquipment equipment)
        {
            if (equipment.InventoryItem == null) return;

            Gear gear = equipment.InventoryItem.Item as Gear;
            foreach (Trait trait in gear.Traits)
            {
                character.RemoveTrait(trait);
            }
        }

        public void ApplyAllEquipmentsTraits()
        {
            if (HasAuthority)
                foreach (CharacterEquipment equipment in Equipments)
                {
                    ApplyEquipmentTraits(equipment);
                }
        }
        protected InventoryItem CreateItem(InventoryItemData data)
        {
            return UIController.Singleton.CreateItem(data);
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
            
            if (inventoryItem != null && inventoryItem.Inventory == null)
            {
                Debug.LogError(gameObject.name + ": inventoryItem.Inventory == null - " + inventoryItem.Item.name);
                return;
            }

            Inventory = inventoryItem.Inventory;
        }
        public virtual void AddUtility(InventoryItem inventoryItem, int duplicatedIndex = 0)
        {
            if (inventoryItem != null && inventoryItem.Inventory == null)
            {
                Debug.LogError(gameObject.name + ": inventoryItem.Inventory == null - " + inventoryItem.Item.name);
            }

            UtilityInventories[duplicatedIndex] = inventoryItem.Inventory;
        }
        public virtual void RemoveBackpack()
        {
            Inventory = null;
        }

        public virtual void RemoveUtility(int duplicatedIndex)
        {
            UtilityInventories[duplicatedIndex] = null;
        }
        public Vector2Int GetWeaponDamageAndPen(List<CharacterTrait> traits)
        {
            Vector2Int defaultValue = new Vector2Int(character.Stats.Strength * 10, 0);

            Gear gear = GetEquippedGearByType(WeaponSlot);
            if (gear == null) return defaultValue;

            Weapon weapon = gear as Weapon;
            if (weapon == null) return defaultValue;

            int damage = weapon.Attack;

            foreach (WeaponModifier modifier in weapon.WeaponModifiers)
            {
                damage += character.Stats.GetStatValue(modifier.Stat) * modifier.Value;
            }

            foreach (CharacterTrait characterTrait in traits)
            {
                damage += characterTrait.Trait.GetAttackChange();
            }

            return new Vector2Int(damage, weapon.DamageClass);
        }

        internal Vector2Int GetArmorDefenseAndPen(List<CharacterTrait> traits)
        {
            Vector2Int defaultValue = new(0, 0);

            Gear gear = GetEquippedGearByType(BodyArmorSlot);
            if (gear == null) return defaultValue;

            Chest bodyArmor = gear as Chest;
            if (bodyArmor == null) return defaultValue;

            int defense = defaultValue.x + bodyArmor.Defense;

            foreach (BodyArmorModifier modifier in bodyArmor.BodyArmorModifiers)
            {
                defense += character.Stats.GetStatValue(modifier.Stat) * modifier.Value;
            }

            foreach (CharacterTrait characterTrait in traits)
            {
                defense += characterTrait.Trait.GetDefenseChange();
            }

            return new Vector2Int(defense, bodyArmor.ArmorClass);
        }

        public Gear GetEquippedGearByType(EquipmentType equipmentType)
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                if (equipment.InventoryItem == null) continue;

                Gear gear = equipment.InventoryItem.Item as Gear;

                if (gear.EquipmentType == equipmentType)
                {
                    return gear;
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

                UIController.Singleton.LootCharacterInventoryUI.SetCharacter(character);

                if (!UIController.Singleton.IsGridsOpen)
                {
                    UIController.Singleton.OpenLootGrids();
                }
                else if (UIController.Singleton.IsGridsOpen)
                {
                    Debug.Log(gameObject.name + ": CharacterGear Interact IsGridsOpen = true");
                    UIController.Singleton.CloseLootGrids();
                }
            }
        }
        private void HandleStopInteraction()
        {
            if (looter == null) return;

            float maxDistance = (float)(looter.Stats.Dexterity + looter.Stats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, looter.transform.position);

            if (distance > maxDistance)
            {
                looter = null;
                UIController.Singleton.CloseAllGrids();
            }
        }
#if UNITY_EDITOR
        void OnValidate()
        {
            // Default Values for the slots
            if (BackpackSlot == null)
                BackpackSlot = FindEquipmentType("Back");

            if (WeaponSlot == null)
                WeaponSlot = FindEquipmentType("Weapon");

            if (BodyArmorSlot == null)
                BodyArmorSlot = FindEquipmentType("Chest");

            if (UtilitySlot == null)
                UtilitySlot = FindEquipmentType("Utility");
            
            // Create index to differentiate duplicated slots
            Dictionary<string, int> qtyBySlotName = new();

            foreach (CharacterEquipment slot in Equipments)
            {
                if (qtyBySlotName.ContainsKey(slot.GearSlotType.name))
                    qtyBySlotName[slot.GearSlotType.name]++;
                else
                    qtyBySlotName[slot.GearSlotType.name] = 0;

                slot.DuplicateIndex = qtyBySlotName[slot.GearSlotType.name];
            }

            if(qtyBySlotName.ContainsKey(UtilitySlot.name))
            {
                UtilityInventories = new List<Inventory>(new Inventory[qtyBySlotName[UtilitySlot.name] + 1]);
            }
        }
        public EquipmentType FindEquipmentType(string equipmentTypeName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:EquipmentType {equipmentTypeName}", new[] { "Assets/Items/Gears/Type" });
            if (guids.Length == 0)
                Debug.LogError($"EquipmentType {equipmentTypeName} not found");
            

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            return (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));
        }
#endif
    }
}

