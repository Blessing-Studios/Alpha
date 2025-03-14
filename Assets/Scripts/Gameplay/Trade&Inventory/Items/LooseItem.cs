using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class LooseItem : NetworkBehaviour, IInteractable
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public InventoryItem InventoryItem;
        protected NetworkVariable<InventoryItemData> data = new(new InventoryItemData(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public Item Item;
        public string Guid;
        public TextMeshPro GuidText;
        private readonly int deferredDespawnTicks = 4;
        public bool CanInteract { get { return true; } }
        public override void OnNetworkSpawn()
        {
            // If Player alrealdy connected, call InitializeLooseItem
            if (GameManager.Singleton.PlayerConnected)
                InitializeLooseItem();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        }
        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                InitializeLooseItem();
            }
        }

        private void InitializeLooseItem()
        {
            if (InventoryItem != null)
                data.Value = InventoryItem.Data;

            if (InventoryItem == null)
            {
                if (HasAuthority)
                {
                    if (Item == null) Debug.LogError(gameObject.name + ": Item is missing");

                    if (Item != null) // Item spawned from LooseItemSpawner Class
                    {
                        InventoryItem = GameManager.Singleton.InventoryController.CreateItem(Item);
                    }

                    data.Value = InventoryItem.Data;
                }

                if (!HasAuthority)
                {
                    InventoryItem = GameManager.Singleton.InventoryController.CreateItem(data.Value);

                    if (InventoryItem == null)
                    {
                        Debug.LogError(gameObject.name + ": Não conseguiu criar InventoryItem");
                    }
                }
            }

            GetComponent<SpriteRenderer>().sprite = InventoryItem.Item.Sprite;
            GetComponent<SphereCollider>().radius = GetComponent<SpriteRenderer>().bounds.size.magnitude / 4;

            // Para debugar
            Guid = InventoryItem.Data.Id.ToString();
            GuidText.text = Guid;

            if (ShowDebug)
                GuidText.gameObject.SetActive(true);
            else
                GuidText.gameObject.SetActive(false);

            InventoryItem.gameObject.SetActive(false);
        }
        public void Interact(Interactor interactor)
        {
            Debug.Log(gameObject.name + "Interact");
            if (interactor.gameObject.TryGetComponent(out CharacterGear character))
            {
                GetOwnership();

                // TODO: Checar se GetOwnership funcionou

                // Try to equip item, if yes, return
                if (character.AddEquipment(InventoryItem))
                {
                    // InventoryItem.gameObject.SetActive(false);
                    // TODO: criar um sistema de pooling para destruir objetos
                    if (NetworkManager.Singleton.NetworkConfig.NetworkTopology == NetworkTopologyTypes.DistributedAuthority)
                    {
                        gameObject.SetActive(false);
                        NetworkObject.DeferDespawn(deferredDespawnTicks, destroy: true);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                    
                    Debug.Log(gameObject.name + "Item equiped: " + character.gameObject.name);
                    
                    return;
                }

                if (character.Inventory == null) return;
                if (character.Inventory.AddItem(InventoryItem))
                {
                    Debug.Log(gameObject.name + "Got Item: " + character.gameObject.name);
                    // Destroy this object
                    // TODO: mudar lógica para usar pooling, criar pooling para LooseItems,

                    if (NetworkManager.Singleton.NetworkConfig.NetworkTopology == NetworkTopologyTypes.DistributedAuthority)
                    {
                        gameObject.SetActive(false);
                        NetworkObject.DeferDespawn(deferredDespawnTicks, destroy: true);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }

                    if (character.Inventory.InventoryGrid)
                    {
                        character.Inventory.InventoryGrid.UpdateFromInventory();
                    }
                }
                else
                {
                    Debug.Log(gameObject.name + "Iem NOT got: " + character.gameObject.name);
                }
            }
        }
        public void GetOwnership()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            GameManager.Singleton.GetOwnership(networkObject);

            // ulong LocalClientId = NetworkManager.Singleton.LocalClientId;
            // if (LocalClientId != OwnerClientId)
            //     ChangeOwnership(LocalClientId);
        }
    }
}

