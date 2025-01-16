using Blessing.Gameplay.Characters;
using Blessing.Gameplay.TradeAndInventory.Containers;
using Blessing.Player;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryController : MonoBehaviour
    {
        public PlayerCharacter PlayerCharacter;
        public Character OtherCharacter;
        public GameObject InventoryCanvas;
        public InventoryGrid PlayerInventoryGrid;
        public GameObject PlayerEquipmentsFrame;
        public CharacterStatsInfo PlayerStatsInfo;
        public InventoryGrid OtherInventoryGrid;
        public GameObject OtherEquipmentsFrame;
        public CharacterStatsInfo OtherStatsInfo;
        public List<BaseGrid> Grids;
        public List<BaseGrid> OtherGrids;
        public IGrid SelectedGrid;

        [SerializeField] public ItemList SpawnableItems;
        public GameObject ItemPrefab { get { return GameManager.Singleton.InventoryItemPrefab; } }
        public NetworkObject LooseItemPrefab { get { return GameManager.Singleton.LooseItemPrefab; } }
        private Transform canvasTransform { get { return InventoryCanvas.transform; } }
        [SerializeField] private InventoryItem selectedItem;
        [SerializeField] private bool isGridsOpen = false;
        public bool IsGridsOpen { get { return isGridsOpen; } }
        private Dictionary<Guid, InventoryItem> inventoryItemDic = new();

        void Awake()
        {
            // GameManager.Singleton.InventoryController = this;
        }

        void Start()
        {
            // Criar lógica para desativar o inventory controller dos outros players nesse Client

            if (InventoryCanvas == null)
            {
                Debug.LogError(gameObject.name + " InventoryCanvas is missing");
            }

            if (PlayerInventoryGrid == null)
            {
                Debug.LogError(gameObject.name + " PlayerInventoryGrid is missing");
            }

            if (PlayerEquipmentsFrame == null)
            {
                Debug.LogError(gameObject.name + " PlayerEquipmentsFrame is missing");
            }

            if (PlayerStatsInfo == null)
            {
                Debug.LogError(gameObject.name + " PlayerStatsInfo is missing");
            }

            if (SpawnableItems == null)
            {
                Debug.LogError(gameObject.name + " SpawnableItems is missing");
            }

            CloseGrids();
            CloseOtherGrids();
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
            CloseGrids();
            CloseOtherGrids();
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


            PlayerStatsInfo.UpdateStatInfo();
            OtherStatsInfo.UpdateStatInfo();
        }

        public void OpenGrids()
        {
            if (!CanOpenGrids()) return;

            foreach (var grid in Grids)
            {
                grid.OpenGrid();
            }

            PlayerEquipmentsFrame.SetActive(true);

            isGridsOpen = true;
        }

        public void CloseGrids()
        {
            foreach (var grid in Grids)
            {
                grid.CloseGrid();
            }

            PlayerEquipmentsFrame.SetActive(false);

            isGridsOpen = false;
        }

        public void OpenOtherGrids()
        {
            if (OtherCharacter != null)
            {
                OtherInventoryGrid.Inventory = OtherCharacter.Gear.Inventory;
                OtherStatsInfo.CharacterStats = OtherCharacter.Gear.GetStats();
                OtherStatsInfo.Initialize();
                OtherEquipmentsFrame.SetActive(true);

                foreach (var grid in OtherGrids)
                {
                    grid.Owner = OtherCharacter.gameObject;
                    grid.InitializeGrid();
                    grid.OpenGrid();
                }
            }

            if (OtherCharacter == null)
            {
                OtherInventoryGrid.InitializeGrid();
                OtherInventoryGrid.OpenGrid();
            }
        }

        public void CloseOtherGrids()
        {
            foreach (var grid in OtherGrids)
            {
                grid.CloseGrid();
                grid.Owner = null;
            }

            OtherStatsInfo.CharacterStats = null;

            OtherEquipmentsFrame.SetActive(false);
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

            selectedItem.RectTransform.SetParent(canvasTransform);
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

        public InventoryItem FindInventoryItem(InventoryItemData data)
        {
            if (inventoryItemDic.ContainsKey(data.Id))
                return inventoryItemDic[data.Id];
            else
                return CreateItem(data);
        }

        public InventoryItem CreateItem(Item item)
        {
            InventoryItem inventoryItem = GetInventoryItem();
            inventoryItem.gameObject.SetActive(true);
            inventoryItem.RectTransform.SetParent(canvasTransform);

            inventoryItem.Set(item);

            inventoryItemDic.Add(inventoryItem.Data.Id, inventoryItem);

            return inventoryItem;

        }

        public InventoryItem CreateItem(InventoryItemData data)
        {
            if (inventoryItemDic.ContainsKey(data.Id))
                return FindInventoryItem(data);

            InventoryItem inventoryItem = GetInventoryItem();
            inventoryItem.gameObject.SetActive(true);
            inventoryItem.RectTransform.SetParent(canvasTransform);

            foreach (Item item in SpawnableItems.Items)
            {
                if (item.Id == data.ItemId)
                {
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

            inventoryItem.RectTransform.SetParent(canvasTransform);
            inventoryItem.Set(SpawnableItems.Items[UnityEngine.Random.Range(0, SpawnableItems.Items.Length)]);

            inventoryItemDic.Add(inventoryItem.Data.Id, inventoryItem);

            selectedItem = inventoryItem;
        }

        private InventoryItem GetInventoryItem()
        {
            return GameManager.Singleton.GetInventoryItem();
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

            return SelectedGrid.GetTileGridPosition(position);
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

            if (SelectedGrid.PlaceItem(selectedItem, position))
                selectedItem = null;
        }

        private void PickUpItem(Vector2Int position)
        {
            selectedItem = SelectedGrid.PickUpItem(position);
        }

        public void MoveItemToGround()
        {
            InventoryItem inventoryItem = selectedItem;

            GameObject owner = PlayerInventoryGrid.Owner;

            Vector3 randomVector = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.2f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f));

            var looseItem = Instantiate(LooseItemPrefab, position: owner.transform.position + randomVector, rotation: owner.transform.rotation);

            looseItem.GetComponent<LooseItem>().InventoryItem = inventoryItem;

            looseItem.Spawn();

            inventoryItem.gameObject.SetActive(false);

            selectedItem = null;
        }
    }
}

