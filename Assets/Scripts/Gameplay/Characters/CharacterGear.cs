using System;
using System.Collections.Generic;
using Blessing.Core.GameEventSystem;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.Characters
{
    public class CharacterGear : NetworkBehaviour
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

        [Header("Events")]
        public GameEvent OnAddEquipment;
        public GameEvent OnRemoveEquipment;
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
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EquipmentNetworkList.OnListChanged += OnEquipmentNetworkListChanged;
        }

        private void OnEquipmentNetworkListChanged(NetworkListEvent<InventoryItemData> changeEvent)
        {
            if(!UpdateLocalList(ref EquipmentLocalList, EquipmentNetworkList)) return;

            if (changeEvent.Type == NetworkListEvent<InventoryItemData>.EventType.Add)
            {
                InventoryItem itemCreated = CreateItem(changeEvent.Value);
                AddEquipment(itemCreated);
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
        public bool AddEquipment(CharacterEquipment equipment, InventoryItem inventoryItem)
        {
            Gear gear = inventoryItem.Item as Gear;
            if (gear == null) return false;
            if (equipment.GearSlotType == gear.GearType)
            {
                if (!equipment.SetEquipment(inventoryItem)) return false;

                ApplyEquipmentTraits(equipment);

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

        public bool AddEquipment(InventoryItem inventoryItem)
        {
            foreach (CharacterEquipment equipment in Equipments)
            {
                if (AddEquipment(equipment, inventoryItem)) return true;
            }

            return false;
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
                        OnRemoveEquipment.Raise(this);
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
            // CharactersEquipments can't repeat
        }

        protected InventoryItem CreateItem( InventoryItemData data)
        {
            return GameManager.Singleton.InventoryController.CreateItem(data);
        }

        protected bool UpdateLocalList(ref List<InventoryItemData> localList, NetworkList<InventoryItemData> networkList)
        {
            return GameManager.Singleton.UpdateLocalList(ref localList, networkList);
        }

        public void SetInventory(InventoryItem inventoryItem)
        {
            Inventory = inventoryItem.Inventory;

            GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = Inventory;
            Inventory.InventoryGrid = GameManager.Singleton.InventoryController.PlayerInventoryGrid;

            GameManager.Singleton.InventoryController.CheckOpenGrids();
        }

        public void UnequipInventory()
        {
            GameManager.Singleton.InventoryController.PlayerInventoryGrid.Inventory = null;

            if (Inventory != null)
            {
                Inventory.InventoryGrid = GameManager.Singleton.InventoryController.OtherInventoryGrid as InventoryGrid;
                Inventory = null;
            }
            

            GameManager.Singleton.InventoryController.CheckOpenGrids();
        }
    }
}

