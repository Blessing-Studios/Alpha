using UnityEngine;
using UnityEditor;
using System;
using Blessing.Characters;

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
        [Tooltip("Damage Class is the penetration power of the damage source")]
        public int DamageClass;
        public int Attack;
        public WeaponModifier[] WeaponModifiers;
    }
}

