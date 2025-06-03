using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Gameplay.Guild;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Player;
using Blessing.UI.Quests;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class UIController : MonoBehaviour
    {
        // TODO: Refatorar lógica
        public static UIController Singleton { get; private set; }
        public CharacterInventoryUI PlayerCharacterInventoryUI;
        public CharacterInventoryUI LootCharacterInventoryUI;
        public TraderInventoryUI TraderInventoryUI;
        public QuestsUI QuestsUI;
        public PlayerHUD PlayerHUD;
        public Character LootCharacter;
        public IGrid SelectedGrid;
        [SerializeField] public ItemList SpawnableItems { get { return GameManager.Singleton.AllItems; } }
        public NetworkObject LooseItemPrefab { get { return GameManager.Singleton.LooseItemPrefab; } }
        public ItemInfoBox ItemInfoBox { get { return GameManager.Singleton.ItemInfoBox; } }
        [SerializeField] private InventoryItem selectedItem;
        [SerializeField] private bool isGridsOpen = false;
        public bool IsGridsOpen { get { return isGridsOpen; } }
        private Dictionary<FixedString64Bytes, InventoryItem> inventoryItemDic = new();

        void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Singleton = this;
            }
        }

        void Start()
        {
            // Criar lógica para desativar o inventory controller dos outros players nesse Client

            if (SpawnableItems == null)
            {
                Debug.LogError(gameObject.name + " SpawnableItems is missing");
            }

            CloseAllGrids();
            PlayerHUD.CloseHUD();
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

        public void SetPlayerCharacter(Character playerCharacter)
        {
            PlayerCharacterInventoryUI.SetCharacter(playerCharacter);

        }
        public void ClearInventoryItemDic()
        {
            inventoryItemDic.Clear();
        }

        private bool CanOpenGrids()
        {
            if (GameManager.Singleton.SceneStarter != null) return true;

            return false;
        }

        public void CloseAllGrids()
        {
            PlayerCharacterInventoryUI.CloseInventoryUI();
            LootCharacterInventoryUI.CloseInventoryUI();
            TraderInventoryUI.CloseInventoryUI();
            ItemInfoBox.CloseInfoBox();

            QuestsUI.CloseQuestsUI();

            // PlayerHUD.CloseHUD();

            SelectedGrid = null;

            GameManager.Singleton.RemoveBlurFromBackground();

            isGridsOpen = false;
        }

        public void ToggleInventoryUI()
        {
            if (!CanOpenGrids()) return;

            if (!isGridsOpen)
                OpenInventoryGrids();
            else
                CloseAllGrids();
        }

        public void SyncInventoryGrids()
        {
            PlayerCharacterInventoryUI.SyncGrids();
            LootCharacterInventoryUI.SyncGrids();
            TraderInventoryUI.SyncGrids();
        }

        public void OpenInventoryGrids()
        {
            isGridsOpen = true;
            PlayerCharacterInventoryUI.OpenInventoryUI();
            GameManager.Singleton.AddBlurToBackground();
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
            ItemInfoBox.CloseInfoBox();

            GameManager.Singleton.RemoveBlurFromBackground();
        }
        public void OpenTraderGrids(Trader trader)
        {
            isGridsOpen = true;

            TraderInventoryUI.OpenInventoryUI(trader);

            GameManager.Singleton.AddBlurToBackground();
        }

        public void CloseTraderGrids()
        {
            isGridsOpen = false;

            TraderInventoryUI.CloseInventoryUI();

            GameManager.Singleton.RemoveBlurFromBackground();
        }

        public void OpenQuestsGrid(List<Quest> quests, Adventurer adventurer)
        {
            isGridsOpen = true;

            QuestsUI.OpenQuestsUI(quests, adventurer);

            GameManager.Singleton.AddBlurToBackground();
        }

        public void CloseQuestsGrid()
        {
            isGridsOpen = false;

            QuestsUI.CloseQuestsUI();

            GameManager.Singleton.RemoveBlurFromBackground();
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

            if (SelectedGrid != null)
                selectedItem.RectTransform.SetParent(SelectedGrid.Canvas.transform, false);

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
                    ItemInfoBox.OpenInfoBox(item, SelectedGrid.ItemHighlight.transform.position);
                }
                else
                {
                    SelectedGrid.RemoveHighlight();
                    ItemInfoBox.CloseInfoBox();
                }
            }
            else if (selectedItem != null)
            {
                ItemInfoBox.CloseInfoBox();
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

        public void HandleUseItem(InventoryItem inventoryItem, int charges = 1)
        {
            if (inventoryItem == null) return;

            // Check if item can be consumed
            Consumable consumable = inventoryItem.Item as Consumable;
            if (consumable != null)
            {
                if (inventoryItem.Data.Stack > 1)
                {
                    foreach (Buff buff in consumable.Buffs)
                    {
                        PlayerCharacterInventoryUI.Character.ApplyBuff(buff);
                    }
                }

                inventoryItem.RemoveFromStack(charges);

                // ReleaseInventoryItem(inventoryItem);
            }
        }
    }
}

