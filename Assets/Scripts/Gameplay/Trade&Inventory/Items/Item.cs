using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Items/Item")]
    public class Item : ScriptableObject
    {
        [Header("Item Info")]
        public string Label;
        public string Description;
        public int Value = 10;
        public int Width = 1;
        public int Height = 1;
        public int MaxStack = 1;
        public float Weight = 1.0f;
        public Sprite Sprite;
        public int Id = 0;
        [ScriptableObjectDropdown(typeof(ItemType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        [SerializeField] private ScriptableObjectReference itemType;
        public ItemType ItemType { get { return itemType.value as ItemType; } set { itemType.value = value;}}

        public virtual void Initialize(InventoryItem inventoryItem)
        {
            //
        }

        public virtual string GetInfo()
        {
            return "";
        }

#if UNITY_EDITOR
        public virtual void Awake()
        {
            string[] guids = AssetDatabase.FindAssets("t:item", new[] { "Assets/Items" });

            if (Id == 0)
                Id = guids.Length;
        }


        [MenuItem("Blessing/GameData/Items/Generate Items Ids")]
        public static void GenerateItemsIds()
        {
            // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
            string[] guids = AssetDatabase.FindAssets("t:item", new[] { "Assets/Items" });

            int incrementId = 1;

            List<Item> items = new();

            foreach (string guid in guids)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));

                string path = AssetDatabase.GUIDToAssetPath(guid);
                Item item = (Item) AssetDatabase.LoadAssetAtPath(path, typeof(Item));

                item.Id = incrementId;

                Debug.Log(item.name + " New Id: " + incrementId);

                items.Add(item);  

                incrementId += 1;

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // Update AllItems List

            // Try to get All Items list
            string[] allItemsGuids = AssetDatabase.FindAssets("t:ItemList AllItems", new[] { "Assets/Items" });

            ItemList allItemList;

            if (allItemsGuids.Length == 0)
            {
                Debug.Log("ItemList AllItems was created");
                
                string path = $"Assets/Items/AllItems.asset";
                allItemList = CreateInstance<ItemList>();
                allItemList.Description = "Lista com todos os items do jogo";
                AssetDatabase.CreateAsset(allItemList, path);
            }
            else
            {
                Debug.Log("AllItems was updated");
                string pathAllITemList = AssetDatabase.GUIDToAssetPath(allItemsGuids[0]);
                allItemList = (ItemList) AssetDatabase.LoadAssetAtPath(pathAllITemList, typeof(ItemList));
            }

            allItemList.Items = items.ToArray();
            EditorUtility.SetDirty(allItemList);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static List<Dictionary<string, object>> GetItemsListFromCSV()
        {
            if (Selection.activeObject == null)
            {
                Debug.LogWarning("Need to Select a CSV file.");
                return null;
            };

            string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(Selection.activeObject));

            if (!IsCSVFile(path))
            {
                Debug.LogWarning("Not a CSV file.");
                return null;
            }

            List<Dictionary<string, object>> rawCSVData = CSVReading.CSVReader.Read(path);

            if (rawCSVData.Count == 0)
            {
                Debug.LogWarning("No entries read from CSV");
                return null;
            }

            return rawCSVData;
        }

        static List<T> GetAll<T>() where T : Item
        {
            string typeName = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets($"t:{typeName}", new[] { "Assets/Items" });

            List<T> items = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                items.Add((T) AssetDatabase.LoadAssetAtPath(path, typeof(T)));
            }

            return items;
        }

        private static bool IsCSVFile(string fullPath)
        {
            return fullPath.ToLower().EndsWith(".csv");
        }
#endif
    }
}