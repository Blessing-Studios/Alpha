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
    [CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Items/Gears/BodyArmor")]
    public class BodyArmor : Gear
    {
        [Header("Weapon Info")]
        [Tooltip("Armor Class is the penetration resistance of the defense source")]
        public int ArmorClass;
        public int Defense;
        public BodyArmorModifier[] BodyArmorModifiers;
    }
}

