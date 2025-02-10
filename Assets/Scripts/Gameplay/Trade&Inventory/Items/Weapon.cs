using UnityEngine;
using UnityEditor;
using System;
using Blessing.Gameplay.Characters;
using UnityEditor.VersionControl;
using System.Collections.Generic;

namespace Blessing.Gameplay.TradeAndInventory
{
    [Serializable]
    public class WeaponModifier
    {
        public Stat Stat;
        public int Value;
    }
    [CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Items/Gears/Weapon")]
    public class Weapon : Gear
    {
        [Header("Weapon Info")]
        
        [Tooltip("Damage Class is the penetration power of the damage source")] public int DamageClass;
        public int Attack;
        public WeaponModifier[] WeaponModifiers;

#if UNITY_EDITOR
        [MenuItem("Blessing/GameData/Items/Generate Weapons from CSV file")]
        static void GenerateWeapons()
        {
            List<Dictionary<string, object>> rawCSVData = GetItemsListFromCSV();
            if (rawCSVData == null) return;

            // Sanitize rawCSVData
            rawCSVData.RemoveAll(item => { return (string)item["Type"] != "Weapon"; });

            bool confirmed = EditorUtility.DisplayDialog("Mass Generate Weapons", $"Are you sure you want to create {rawCSVData.Count} entries? This may take a while.", "Yes!", "No.");

            if (!confirmed) return;

            // Generate or Update weapons

            List<Weapon> allWeapons = GetAllWeapons();

            foreach (Dictionary<string, object> weaponData in rawCSVData)
            {
                List<int> weaponsFound = new();
                // checa se arma já foi criada antes
                
                for (int i = 0; i < allWeapons.Count; i++)
                {
                    if (allWeapons[i].name == (string) weaponData["Name"])
                    {
                        // Update Weapon with CSV file data
                        weaponsFound.Add(i);
                    }

                    // If found more than one weapon, trigger error message
                    if (weaponsFound.Count > 1)
                    {
                        Debug.LogError("Found Weapons with the same name: " + allWeapons[i].name);
                    }
                }

                // If weapon WAS found, update found weapon
                if (weaponsFound.Count > 0)
                {
                    foreach (int i in weaponsFound)
                    {
                        Debug.Log("Weapon Updated: " + weaponData["Name"]);

                        allWeapons[i] = UpdateWeapon(allWeapons[i], weaponData);
                        EditorUtility.SetDirty(allWeapons[i]);
                        AssetDatabase.SaveAssets();
                    }
                }

                // If weapon WAS NOT found, create a new Scriptable file from weaponData
                if (weaponsFound.Count == 0)
                {
                    // Create new weapon
                    Debug.Log("Weapon Created: " + weaponData["Name"]);
                    
                    string path = $"Assets/Items/Gears/Weapons/{weaponData["Name"]}.asset";
                    Weapon weapon = CreateInstance<Weapon>();
                    AssetDatabase.CreateAsset(weapon, path);

                    weapon = UpdateWeapon(weapon, weaponData);

                    EditorUtility.SetDirty(weapon);
                    AssetDatabase.SaveAssets();
                    
                }
            }
            AssetDatabase.Refresh();
            GenerateItemsIds();
        }

        static List<Weapon> GetAllWeapons()
        {
            string[] guids = AssetDatabase.FindAssets("t:weapon", new[] { "Assets/Items/Gears/Weapons" });

            List<Weapon> weapons = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                weapons.Add((Weapon) AssetDatabase.LoadAssetAtPath(path, typeof(Weapon)));
            }

            return weapons;
        }

        static Weapon UpdateWeapon(Weapon weapon, Dictionary<string, object> weaponData)
        {
            // Item info
            weapon.Label = (string) weaponData["Label"];
            weapon.Value = (int) weaponData["Value"];
            weapon.Width = (int)weaponData["Width"];
            weapon.Height = (int)weaponData["Height"];
            weapon.Weight = (int) weaponData["Weight"];

            // Gear info
            string[] guids = AssetDatabase.FindAssets("t:EquipmentType Weapon", new[] { "Assets/Items/Gears/Type" });
            string pathAsset = AssetDatabase.GUIDToAssetPath(guids[0]);
            weapon.GearType = (EquipmentType) AssetDatabase.LoadAssetAtPath(pathAsset, typeof(EquipmentType));

            // weapon.Traits TODO:

            // Weapon info
            weapon.DamageClass = (int) weaponData["DC"];
            weapon.Attack = (int) weaponData["Attack"];

            string[] stats = weaponData["Stats"].ToString().Split(',');
            string[] stringMultiplier = weaponData["Multiplier"].ToString().Split(',');
            int[] multiplier = Array.ConvertAll(stringMultiplier, int.Parse);

            List<WeaponModifier> modifiers = new();

            for (int i = 0; i < stats.Length; i++)
            {
                Stat modifiedStat = (Stat) Enum.Parse(typeof(Stat), stats[i]);
                modifiers.Add(new WeaponModifier() { Stat = modifiedStat, Value = multiplier[i] });
            }

            weapon.WeaponModifiers = modifiers.ToArray();
            return weapon;
        }
    }
#endif
}