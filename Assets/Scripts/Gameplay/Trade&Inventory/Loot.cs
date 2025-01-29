using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [RequireComponent(typeof(Inventory))]
    public class Loot : MonoBehaviour, IInteractable
    {
        public int GridSizeWidth = 5;
        public int GridSizeHeight = 5;
        private Character looter;
        [field: SerializeField] public Inventory Inventory { get; private set; }
        public bool CanInteract { get { return true; } }
        void Awake()
        {
            Inventory = GetComponent<Inventory>();
        }

        void Start()
        {
            Inventory.SetNetworkVariables(GridSizeWidth, GridSizeHeight);
            Inventory.Initialize();

            if (Inventory.InventoryGrid == null)
            {
                Debug.Log(gameObject.name + " InventoryGrid can't be null");
            }
        }

        void Update()
        {
            HandleStopInteraction();
        }

        public void Interact(Interactor interactor)
        {
            if (interactor.gameObject.TryGetComponent(out Character looter))
            {
                if (!GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    Debug.Log(gameObject.name + " OpenGrids");
                    OpenGrids(looter);
                }
                else if (GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    Debug.Log(gameObject.name + " CloseGrids");
                    CloseGrids();
                }
            }
        }
        private void HandleStopInteraction()
        {
            if (looter == null) return;

            if (!Inventory.InventoryGrid.IsOpen) return;

            float maxDistance = (float ) (looter.CharacterStats.Dexterity + looter.CharacterStats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, looter.transform.position);

            if (distance > maxDistance)
            {
                CloseGrids();
            }
        }

        private void OpenGrids(Character looter)
        {
            if (Inventory.InventoryGrid.Inventory != Inventory)
            {
                CloseGrids();
                Inventory.InventoryGrid.Inventory = Inventory;
            }
            
            this.looter = looter;
            GameManager.Singleton.InventoryController.OpenAllGrids();
        }

        private void CloseGrids()
        {
            looter = null;
            GameManager.Singleton.InventoryController.CloseAllGrids();
        }
    }
}

