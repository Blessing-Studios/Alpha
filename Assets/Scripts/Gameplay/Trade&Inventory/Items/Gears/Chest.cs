using UnityEngine;
using UnityEditor;
using System;
using Blessing.Gameplay.Characters;

namespace Blessing.Gameplay.TradeAndInventory
{
    [Serializable]
    public class BodyArmorModifier
    {
        public Stat Stat;
        public int Value;
    }
    [CreateAssetMenu(fileName = "Chest", menuName = "Scriptable Objects/Items/Gears/Chest")]
    public class Chest : Gear
    {
        [Header("Chest Info")]
        [Tooltip("Armor Class is the penetration resistance of the defense source")]
        public int ArmorClass;
        public int Defense;
        public BodyArmorModifier[] BodyArmorModifiers;

        public override string GetInfo()
        {
            return $"Defense {Defense} Class {ArmorClass}";
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            string[] guids = AssetDatabase.FindAssets("t:EquipmentType Chest", new[] { "Assets/Items/Gears/Type" });

            if (guids.Length == 0)
            {
                Debug.LogError("EquipmentType Chest not found");
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            EquipmentType = (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));   
        }
#endif
    }
}

