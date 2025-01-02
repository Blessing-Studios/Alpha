using System;
using System.Collections.Generic;
using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blessing.Gameplay.Characters
{
    public class CharacterInventory : Inventory
    {
        [Header("Character")]
        [SerializeField] protected int gold;
        public TextMeshProUGUI GoldText;
        public int Gold { get { return gold; } }
        //Checar se Gold é positivo

        public List<CharacterEquipment> Equipments;

        public bool SpendGold(int amount)
        {
            if (amount < 0) return false;

            if (gold < amount) return false;

            gold -= amount;
            return true;
        }

        public bool GainGold(int amount)
        {
            if (amount < 0) return false;

            gold += amount;
            return true;
        }

        public void ValidateEquipments()
        {
            // TODO:
        }
    }
}

