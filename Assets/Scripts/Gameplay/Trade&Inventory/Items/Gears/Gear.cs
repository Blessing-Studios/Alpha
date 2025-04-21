using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Gear", menuName = "Scriptable Objects/Items/Gears")]
    public class Gear : Item
    {
        [Header("Gear Info")]
        [ScriptableObjectDropdown(typeof(EquipmentType), grouping = ScriptableObjectGrouping.ByFolderFlat)] 
        [SerializeField] private ScriptableObjectReference equipmentType;
        public EquipmentType EquipmentType { get { return equipmentType.value as EquipmentType; } set { equipmentType.value = value;}}
        public List<Trait> Traits;
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemType Gear", new[] { "Assets/Items/Type" });

            if (guids.Length == 0)
            {
                Debug.Log("ItemType Gear not found");
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            ItemType = (ItemType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(ItemType));   
        }

        public EquipmentType FindEquipmentType(string equipmentTypeName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:EquipmentType {equipmentTypeName}", new[] { "Assets/Items/Gears/Type" });
            if (guids.Length == 0)
                Debug.LogError($"EquipmentType {equipmentTypeName} not found");
            

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            return (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));
        }
#endif
    }
}

