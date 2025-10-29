using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Coin", menuName = "Scriptable Objects/Items/Coins")]
    public class Coin : Item
    {
        // [Header("Coin Info")]
        
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemType Coin", new[] { "Assets/Items/Type" });

            if (guids.Length == 0)
            {
                Debug.Log("ItemType Coin not found");
                return;
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            ItemType = (ItemType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(ItemType));
        }
#endif
    }
}