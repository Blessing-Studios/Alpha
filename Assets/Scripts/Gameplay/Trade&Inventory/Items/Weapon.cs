using UnityEngine;
using UnityEditor;
using System;

namespace Blessing.Gameplay.TradeAndInventory
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Items/Gears/Weapon")]
    public class Weapon : Gear
    {
        [Header("Weapon Info")]
        public int Damage;
    }
}

