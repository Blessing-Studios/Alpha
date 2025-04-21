using UnityEngine;
using UnityEditor;
using System;
using Blessing.Gameplay.Characters;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Back", menuName = "Scriptable Objects/Items/Gears/Back")]
    public class Back : Gear
    {
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            string[] guids = AssetDatabase.FindAssets("t:EquipmentType Back", new[] { "Assets/Items/Gears/Type" });

            if (guids.Length == 0)
            {
                Debug.LogError("EquipmentType Back not found");
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            EquipmentType = (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));   
        }
#endif
    }
}

