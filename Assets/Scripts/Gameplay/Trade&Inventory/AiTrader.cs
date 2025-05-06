using System;
using System.Collections.Generic;
using Blessing.Ai;
using Blessing.Ai.Goap;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.AI.Goap
{
    [RequireComponent(typeof(AiCharacterGear))]
    public class AiTrader : AiAgent, ICharacterAgent, IInteractable
    {
        [Space(10f)]
        [Header("Ai Trader")]
        public Character Customer;
        [field: SerializeField] public Vector3 MinRange { get; private set;}
        public Inventory Inventory { get { return AiCharacter.Gear.Inventory; } }
        [SerializeField] private FixedString64Bytes reservedItemGuid;
        private Vector2Int reservedItemPosition;
        protected Animator animator;
        protected int isOpenHash;
        private AiMovementController aiMovementController;
        [field: SerializeField] public  AiCharacter AiCharacter { get; private set; }
        public bool CanInteract { get { return AiCharacter.Health.IsAlive; } }

        // Para debugar
        [SerializeField] private InventoryItem reservedItem;
        
        public override void Awake()
        {
            base.Awake();
            aiMovementController = GetComponent<AiMovementController>();
            animator = GetComponent<Animator>();

            AiCharacter = GetComponent<AiCharacter>();

            isOpenHash = Animator.StringToHash("IsOpen");
        }

        public override void Start()
        {
            base.Start();
            // Inventory.SetNetworkVariables(GridSizeWidth, GridSizeHeight);
            // Inventory.Initialize();

            // if (Inventory.InventoryGrid == null)
            // {
            //     Debug.Log(gameObject.name + " InventoryGrid can't be null");
            // }
        }

        void Update()
        {
            HandleStopInteraction();
        }

        private void HandleStopInteraction()
        {
            if (Customer == null) return;

            if (!Inventory.InventoryGrid.IsOpen) return;

            float maxDistance = (float ) (Customer.Stats.Dexterity + Customer.Stats.Luck) / 3;

            float distance = Vector3.Distance(transform.position, Customer.transform.position);

            if (distance > maxDistance)
            {
                CloseGrids();
            }
        }
        public void Interact(Interactor interactor)
        {
            // TODO: precisa refazer
            if (interactor.gameObject.TryGetComponent(out Character customer))
            {
                Inventory.GetOwnership();
                // if (!Inventory.InventoryGrid.IsOpen)
                // {
                //     OpenTrader(customer);
                // }
                // else if (Inventory.InventoryGrid.IsOpen)
                // {
                //     CloseTrader();
                // }

                // UIController.Singleton.LootCharacter = null;

                if (!UIController.Singleton.IsGridsOpen)
                {
                    OpenGrids(customer);

                    if (animator.GetBool(isOpenHash)) return;

                    animator.SetTrigger("Open");
                    animator.SetBool(isOpenHash, true);
                }
                else if (UIController.Singleton.IsGridsOpen)
                {
                    CloseGrids();

                    if (!animator.GetBool(isOpenHash)) return;

                    animator.SetTrigger("Close");
                    animator.SetBool(isOpenHash, false);
                }
            }
        }

        private void OpenGrids(Character customer)
        {
            if (Inventory.InventoryGrid.Inventory != Inventory)
            {
                CloseGrids();
                Inventory.InventoryGrid.Inventory = Inventory;
            }
            
            Customer = customer;
            UIController.Singleton.OpenAllGrids();
        }

        private void CloseGrids()
        {
            Customer = null;
            UIController.Singleton.CloseAllGrids();
        }

        // private void OpenTrader(Character customer)
        // {
        //     this.Customer = customer;
        //     Inventory.InventoryGrid.OpenGrid();

        //     if (animator.GetBool(isOpenHash)) return;

        //     animator.SetTrigger("Open");
        //     animator.SetBool(isOpenHash, true);
        // }

        // private void CloseTrader()
        // {
        //     Customer = null;
        //     Inventory.InventoryGrid.CloseGrid();

        //     if (!animator.GetBool(isOpenHash)) return;

        //     animator.SetTrigger("Close");
        //     animator.SetBool(isOpenHash, false);
        // }

        public void OnAddItem(Component component, object data)
        {
            if (Customer == null) return;

            if (data is not InventoryItem)
            {
                return;
            }

            InventoryItem item = data as InventoryItem;
            
            // Temporariamente comentado
            // if (component.gameObject == Customer.gameObject
            //     && item.Data.Id == reservedItemGuid)

            if (item.Data.Id == reservedItemGuid)
            {
                PurchaseItem(item);
            }

            if (component.gameObject == Inventory.gameObject
                && item.Data.Id != reservedItemGuid)
            {
                SellItem(item);
            }
        }

        public void OnRemoveItem(Component component, object data)
        {
            if (component.gameObject != Inventory.gameObject) return;

            if (Customer == null) return;

            if (data is not InventoryItem) return;

            InventoryItem item = data as InventoryItem;
            
            ReserveItem(item);

            ShowItemInfo(item);
        }

        private void ShowItemInfo(InventoryItem item)
        {
            // Show message with info and price of the item
            Debug.Log(gameObject.name + "Item Id: " + item.Item.Id);
            Debug.Log(gameObject.name + "Item Name: " + item.Item.name);
            Debug.Log(gameObject.name + "Item Value: " + item.Value);

            // Check if player has Gold to buy,
            bool canBuyItem = item.Value < Customer.Gear.Gold;
            if (!canBuyItem)
            {
                // Show message that the player is trying to buy item but doesn't have gold
            }
        }

        private void ReserveItem(InventoryItem item)
        {
            reservedItemGuid = item.Data.Id;
            reservedItemPosition = item.GridPosition;
            reservedItem = item;
        }

        private void CleanReserveItem()
        {
            reservedItemGuid = Guid.Empty.ToString();
            reservedItemPosition = Vector2Int.zero;
            reservedItem = null;
        }


        private void PurchaseItem(InventoryItem item)
        {
            // Get Gold from player
            bool itemBought = Customer.Gear.SpendGold(item.Value);

            if (!itemBought)
            {
                // If failed, show message that the item was not bought
                // Block Item from being bought
                CancelPurchase(item);
                return;
            }
            
            // If success, show message that item was bought
            Debug.Log(gameObject.name + "PurchaseItem Customer: " + Customer.gameObject.name);

            CleanReserveItem();
        }

        private void SellItem(InventoryItem item)
        {
            bool itemSell = Customer.Gear.GainGold(item.Value);

            if (!itemSell)
            {
                CancelSell(item);
                return;
            }

            // If success, show message that item was bought
            Debug.Log(gameObject.name + "SellItem Seller: " + Customer.gameObject.name);
        }

        private void CancelPurchase(InventoryItem item)
        {
            // Remove Item from buyer inventory
            Customer.Gear.Inventory.RemoveItem(item);

            // Add Item to original place from trader
            Inventory.InventoryGrid.PlaceItem(item, reservedItemPosition);
            Debug.Log(gameObject.name + "CancelPurchase: " + Customer.gameObject.name);
        }

        private void CancelSell(InventoryItem item)
        {
            // Remove Item from Trader inventory
            Inventory.RemoveItem(item);

            // Add Item to original seller
            Customer.Gear.Inventory.AddItem(item);
        }

        public override void SetTarget(GameObject target)
        {
            Target = target;
        }

        public override void OnMovementInput(Vector3 input)
        {
            // aiMovementController.HandleAiMovement(Vector2.zero);
        }

        public override void OnActionInput(string input)
        {
            Invoke(input, 0);
        }

        public override object GetValue(string field)
        {
            // Método temporário
            return true;
        }
        private void AttackAction()
        {
            //
        }
    }
}
