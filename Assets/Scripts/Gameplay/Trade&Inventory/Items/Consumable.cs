using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Items/Consumables")]
    public class Consumable : Item
    {
        [Header("Consumable Info")]
        public Buff[] Buffs;
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemType Consumable", new[] { "Assets/Items/Type" });

            if (guids.Length == 0)
            {
                Debug.Log("ItemType Consumable not found");
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            ItemType = (ItemType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(ItemType));   
        }
#endif
    }
}