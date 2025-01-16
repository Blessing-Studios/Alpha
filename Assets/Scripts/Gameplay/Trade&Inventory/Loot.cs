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
                this.looter = looter;
                Inventory.InventoryGrid.Inventory = Inventory;

                if (!GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    GameManager.Singleton.InventoryController.OpenAllGrids();
                }
                else if (GameManager.Singleton.InventoryController.IsGridsOpen)
                {
                    GameManager.Singleton.InventoryController.CloseAllGrids();
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
                looter = null;
                GameManager.Singleton.InventoryController.CloseAllGrids();
                Inventory.InventoryGrid.Inventory = null;
            }
        }
    }
}

