using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    // TODO: Precisa refazer
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
                
            }
        }
        private void HandleStopInteraction()
        {
            if (looter == null) return;

            if (!Inventory.InventoryGrid.IsOpen) return;

            float maxDistance = (float ) (looter.Stats.Dexterity + looter.Stats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, looter.transform.position);

            if (distance > maxDistance)
            {
                // CloseGrids();
            }
        }
    }
}

