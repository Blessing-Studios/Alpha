using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class LooseItem : NetworkBehaviour, IInteractable
    {
        public InventoryItem InventoryItem;

        protected NetworkVariable<InventoryItemData> data = new(new InventoryItemData(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public Item Item;
        public String Guid;
        private readonly int deferredDespawnTicks = 4;
        public override void OnNetworkSpawn()
        {
            if (InventoryItem != null)
                data.Value = InventoryItem.Data;

            if (InventoryItem == null)
            {
                InitializeLooseItem();
            }

            GetComponent<SpriteRenderer>().sprite = InventoryItem.Item.Sprite;
            Guid = InventoryItem.Data.Id.ToString();
        }

        private void InitializeLooseItem()
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
        public void Interact(Interactor interactor)
        {
            Debug.Log(gameObject.name + "Interact");
            if (interactor.gameObject.TryGetComponent(out CharacterInventory character))
            {
                GetOwnership();

                Debug.Log(gameObject.name + "Interact TryGet Entrou: ");

                if (character.InventoryGrid.PlaceItem(InventoryItem))
                {
                    Debug.Log(gameObject.name + "Got Item: " + character.gameObject.name);

                    // Destroy this object
                    // TODO: mudar lógica para usar pooling
                    NetworkObject.DeferDespawn(deferredDespawnTicks, destroy: true);
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

