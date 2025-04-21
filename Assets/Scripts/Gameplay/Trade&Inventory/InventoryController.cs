using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Player;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Teste CharacterInventoryUI")]
        public CharacterInventoryUI PlayerCharacterInventoryUI;
        public CharacterInventoryUI LootCharacterInventoryUI;
        public Character LootCharacter;
        public IGrid SelectedGrid;
        [SerializeField] public ItemList SpawnableItems { get { return GameManager.Singleton.AllItems; } }
        public NetworkObject LooseItemPrefab { get { return GameManager.Singleton.LooseItemPrefab; } }
        [SerializeField] private InventoryItem selectedItem;
        [SerializeField] private bool isGridsOpen = false;
        public bool IsGridsOpen { get { return isGridsOpen; } }
        private Dictionary<FixedString64Bytes, InventoryItem> inventoryItemDic = new();
        public void ClearInventoryItemDic()
        {
            inventoryItemDic.Clear();
        }

        void Awake()
        {
            // GameManager.Singleton.InventoryController = this;
        }

        void Start()
        {
            // Criar lógica para desativar o inventory controller dos outros players nesse Client

            if (SpawnableItems == null)
            {
                Debug.LogError(gameObject.name + " SpawnableItems is missing");
            }

            CloseAllGrids();
        }

        void Update()
        {
            // Fazer um check para ver se tem algum inventário aberto

            HandleItemDrag();

            HandleHighlight();

            if (Input.GetMouseButtonDown(0))
            {
                LeftMouseButtonPress();
            }
        }

        private bool CanOpenGrids()
        {
            if (GameManager.Singleton.SceneStarter != null) return true;

            return false;
        }

        public void OpenAllGrids()
        {
            OpenGrids();
            OpenOtherGrids();
        }

        public void CloseAllGrids()
        {
            PlayerCharacterInventoryUI.CloseInventoryUI();
            LootCharacterInventoryUI.CloseInventoryUI();
            
            SelectedGrid = null;
        }

        public void ToggleGrids()
        {
            if (!CanOpenGrids()) return;

            if (!isGridsOpen)
                OpenGrids();
            else
                CloseGrids();
        }

        public void SyncGrids()
        {
            PlayerCharacterInventoryUI.SyncGrids();
            LootCharacterInventoryUI.SyncGrids();

            // TODO: Refatorar
            if (isGridsOpen)
            {
                OpenGrids();
                OpenOtherGrids();
            }
            else
            {
                CloseGrids();
                CloseOtherGrids();
            }
        }

        public void OpenGrids()
        {
            Debug.Log(gameObject.name + ": OpenGrids");
            if (!CanOpenGrids()) return;
            Debug.Log(gameObject.name + ": OpenGrids Passou");

            // foreach (var grid in Grids)
            // {
            //     grid.OpenGrid();
            // }

            // PlayerEquipmentsFrame.SetActive(true);
            isGridsOpen = true;

            GameManager.Singleton.AddBlurToBackground();

            // Teste
            PlayerCharacterInventoryUI.OpenInventoryUI();
        }

        public void CloseGrids()
        {
            // foreach (var grid in Grids)
            // {
            //     grid.CloseGrid();
            // }

            // PlayerEquipmentsFrame.SetActive(false);
            isGridsOpen = false;

            GameManager.Singleton.RemoveBlurFromBackground();

            // Teste
            PlayerCharacterInventoryUI.CloseInventoryUI();
        }
        public void OpenLootGrids()
        {
            isGridsOpen = true;

            PlayerCharacterInventoryUI.OpenInventoryUI();
            LootCharacterInventoryUI.OpenInventoryUI();

            GameManager.Singleton.AddBlurToBackground();
        }

        public void CloseLootGrids()
        {
            isGridsOpen = false;

            PlayerCharacterInventoryUI.CloseInventoryUI();
            LootCharacterInventoryUI.CloseInventoryUI();

            GameManager.Singleton.RemoveBlurFromBackground();
        }

        public void OpenOtherGrids()
        {
            
        }

        public void CloseOtherGrids()
        {
            
        }

        public void RotateItem()
        {
            if (selectedItem == null) return;

            selectedItem.Rotate();
        }

        public void RemoveSelectedItem()
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            selectedItem.gameObject.SetActive(false); // Temporário
            selectedItem = null;
        }

        public void InsertRandomItem()
        {
            CreateRandomItem();
            if (selectedItem != null) PlaceItem();
        }

        private void HandleItemDrag()
        {
            if (selectedItem == null) return;

            selectedItem.RectTransform.SetParent(PlayerCharacterInventoryUI.Canvas.transform, false);
            selectedItem.RectTransform.position = Input.mousePosition;
        }
        private void HandleHighlight()
        {

            if (SelectedGrid == null) return;

            Vector2Int position = GetTileGridPosition();

            // If the positions are equal, doesn't need to recalculate
            if (SelectedGrid.HighlightPosition == position)
            {
                return;
            }

            if (selectedItem == null)
            {
                InventoryItem item = SelectedGrid.GetItem(position);
                if (item != null)
                {
                    SelectedGrid.SetHighlight(item);
                }
                else
                {
                    SelectedGrid.RemoveHighlight();
                }
            }
            else if (selectedItem != null)
            {
                SelectedGrid.SetHighlight(selectedItem, GetTileGridPosition());
            }
        }

        public InventoryItem FindInventoryItem(InventoryItemData data, bool createNew = true)
        {
            if (inventoryItemDic.ContainsKey(data.Id))
                return inventoryItemDic[data.Id];
            else if (createNew)
                return CreateItem(data);
            
            return null;
        }

        public InventoryItem CreateItem(Item item)
        {
            InventoryItem inventoryItem = GetInventoryItem();
            inventoryItem.gameObject.SetActive(true);
            inventoryItem.RectTransform.SetParent(PlayerCharacterInventoryUI.Canvas.transform, false);

            inventoryItem.Set(item);

            inventoryItemDic.Add(inventoryItem.Data.Id, inventoryItem);

            return inventoryItem;
        }

        public InventoryItem CreateItem(InventoryItemData data, Inventory inventory = null)
        {
            if (inventoryItemDic.ContainsKey(data.Id))
                return FindInventoryItem(data);

            InventoryItem inventoryItem = GetInventoryItem();
            inventoryItem.gameObject.SetActive(true);
            inventoryItem.RectTransform.SetParent(PlayerCharacterInventoryUI.Canvas.transform, false);

            foreach (Item item in SpawnableItems.Items)
            {
                if (item.Id == data.ItemId)
                {
                    if (inventory != null) inventoryItem.Inventory = inventory;
                    
                    inventoryItem.Set(item, data);
                    inventoryItemDic.Add(inventoryItem.Data.Id, inventoryItem);

                    return inventoryItem;
                }
            }

            return null;
        }

        public void CreateRandomItem() // Função temporário para teste
        {
            if (SelectedGrid == null) return;

            if (selectedItem != null) return;

            InventoryItem inventoryItem = GetInventoryItem();

            inventoryItem.RectTransform.SetParent(PlayerCharacterInventoryUI.Canvas.transform, false);
            inventoryItem.Set(SpawnableItems.Items[UnityEngine.Random.Range(0, SpawnableItems.Items.Length)]);

            inventoryItemDic.Add(inventoryItem.Data.Id, inventoryItem);

            selectedItem = inventoryItem;
        }

        private InventoryItem GetInventoryItem()
        {
            return GameManager.Singleton.GetInventoryItem();
        }

        private void ReleaseInventoryItem(InventoryItem inventoryItem)
        {
            GameManager.Singleton.ReleaseInventoryItem(inventoryItem);
        }

        private void LeftMouseButtonPress() // Organizar
        {
            if (SelectedGrid == null && selectedItem != null)
            {
                MoveItemToGround();
                return;
            }

            if (SelectedGrid == null) return;

            Vector2Int tileGridPosition = GetTileGridPosition();

            if (selectedItem == null)
            {
                PickUpItem(tileGridPosition);
            }
            else
            {
                PlaceItem(tileGridPosition);
            }
        }

        public Vector2Int GetTileGridPosition()
        {
            Vector2 position = Input.mousePosition;

            if (selectedItem != null)
            {
                position.x -= (selectedItem.Width - 1) * BaseGrid.TileSizeWidth / 2;
                position.y += (selectedItem.Height - 1) * BaseGrid.TileSizeHeight / 2;
            }

            return SelectedGrid.GetTileGridPosition(position, SelectedGrid.Canvas.scaleFactor);
        }
        private void PlaceItem()
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            if (SelectedGrid.PlaceItem(selectedItem))
                selectedItem = null;
        }
        private void PlaceItem(Vector2Int position)
        {
            if (selectedItem == null) return;
            if (SelectedGrid == null) return;

            if (SelectedGrid.PlaceItem(selectedItem, position)) selectedItem = null;
        }

        private void PickUpItem(Vector2Int position)
        {
            selectedItem = SelectedGrid.PickUpItem(position);
        }

        public void MoveItemToGround()
        {
            InventoryItem inventoryItem = selectedItem;

            // GameObject owner = PlayerInventoryGrid.Owner;
            GameObject owner = PlayerCharacterInventoryUI.Character.gameObject;

            Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.2f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));

            var looseItem = Instantiate(LooseItemPrefab, position: owner.transform.position + randomVector, rotation: owner.transform.rotation);

            looseItem.GetComponent<LooseItem>().InventoryItem = inventoryItem;

            looseItem.Spawn();

            inventoryItem.gameObject.SetActive(false);

            selectedItem = null;
        }

        public void ConsumeSelectedItem()
        {
            if (selectedItem == null) return;
            
            // Check if item can be consumed
            Consumable consumable = selectedItem.Item as Consumable;
            if (consumable == null) return;

            // Apply item buff to Player Character
            foreach (Buff buff in consumable.Buffs)
            {
                PlayerCharacterInventoryUI.Character.ApplyBuff(buff);
            }

            // Send inventory Item back to pool
            InventoryItem inventoryItem = selectedItem;
            
            ReleaseInventoryItem(inventoryItem);

            selectedItem = null;

        }
    }
}

