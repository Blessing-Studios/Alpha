using System;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Collections;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(Character))]
    public class CharacterTrader : Trader
    {
        protected Character character;
        protected override void Awake()
        {
            base.Awake();

            character = GetComponent<Character>();
        }

        private void SetInventory()
        {
            Inventory = character.Gear.Inventory;
        }
        public override void Interact(Interactor interactor)
        {
            SetInventory();
            
            base.Interact(interactor);
        }
    }
}
