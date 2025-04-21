using Blessing.Gameplay.TradeAndInventory;
using UnityEditor;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Utility", menuName = "Scriptable Objects/Items/Gears/Utility")]
    public class Utility : Gear
    {
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            string[] guids = AssetDatabase.FindAssets("t:EquipmentType Utility", new[] { "Assets/Items/Gears/Type" });

            if (guids.Length == 0)
            {
                Debug.LogError("EquipmentType Utility not found");
            }

            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            EquipmentType = (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));   
        }
#endif
    }
}
