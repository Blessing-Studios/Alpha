using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Items/Item")]
    public class Item : ScriptableObject
    {
        [Header("Item Info")]
        public string Label;
        public int Value = 10;
        public int Width = 1;
        public int Height = 1;
        public float Weight = 1.0f;
        public Sprite Sprite;
        public int Id = 0;

        public virtual void Initialize(InventoryItem inventoryItem)
        {
            //
        }

#if UNITY_EDITOR
        public void Awake()
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

            foreach (string guid in guids)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));

                string path = AssetDatabase.GUIDToAssetPath(guid);
                Item item = (Item) AssetDatabase.LoadAssetAtPath(path, typeof(Item));

                item.Id = incrementId;

                Debug.Log(item.name + " New Id: " + incrementId);

                incrementId += 1;

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
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