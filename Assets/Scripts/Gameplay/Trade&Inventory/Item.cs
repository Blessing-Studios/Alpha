using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
    {
        public int Value = 10;
        public int Width = 1;
        public int Height = 1;
        public Sprite Sprite;
        public int Id = 0;

        #if UNITY_EDITOR
        public void Awake()
        {
            string[] guids = AssetDatabase.FindAssets("t:item", new[] {"Assets/Scripts/Gameplay/Trade&Inventory/Items"});

            if (Id == 0)
                Id = guids.Length;
        }


        [MenuItem("Blessing/GameData/Items/Generate Items Ids")]
        static void GenerateItemsIds()
        {
            // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
            string[] guids = AssetDatabase.FindAssets("t:item", new[] {"Assets/Scripts/Gameplay/Trade&Inventory/Items"});

            int incrementId = 1;

            foreach (string guid in guids)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));

                string path = AssetDatabase.GUIDToAssetPath(guid);
                Item item = (Item)AssetDatabase.LoadAssetAtPath(path, typeof(Item));
                
                item.Id = incrementId;

                Debug.Log(item.name + " New Id: " + incrementId);

                incrementId += 1;

                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Debug.Log("Create new Asset");
                // yourObject = ScriptableObject.CreateInstance<Item>();
                // AssetDatabase.CreateAsset(yourObject, @"Assets\SavedAsset.asset");
            }
        }
        #endif
    }
}