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
        public Character Looter;
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
            HandleAutoClose();
        }

        private void HandleAutoClose()
        {
            if (Looter == null) return;

            if (!Inventory.InventoryGrid.IsOpen) return;

            float maxDistance = (float ) (Looter.CharacterStats.Dexterity + Looter.CharacterStats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, Looter.transform.position);

            if (distance > maxDistance)
            {
                Looter = null;
                Inventory.InventoryGrid.CloseGrid();
            }
        }

        public void Interact(Interactor interactor)
        {
            if (interactor.gameObject.TryGetComponent(out Character looter))
            {
                Looter = looter;
                if (!Inventory.InventoryGrid.IsOpen)
                {
                    Inventory.GetOwnership();
                    Inventory.InventoryGrid.ToggleGrid();
                }
                else if (Inventory.InventoryGrid.IsOpen)
                {
                    Inventory.InventoryGrid.ToggleGrid();
                }
            }
        }
    }
}

