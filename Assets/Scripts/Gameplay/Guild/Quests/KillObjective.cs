using System;
using System.Collections.Generic;
using Blessing.Gameplay.TradeAndInventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay.Guild.Quests
{
    [Serializable]
    public class KillObjective : Objective
    {
        public override ObjectiveType Type { get {return ObjectiveType.Kill; } }
        public Item[] Trophies;
        public int Quantity;
        public KillObjective(Item[] trophies, int quantity)
        {
            Trophies = trophies;
            Quantity = quantity;
        }
        public override bool Validate(Adventurer adventurer)
        {
            // Check if character has Trophies
            Inventory inventory = adventurer.Character.Gear.Inventory;
            int questQty = 0;
            if (inventory != null)
            {
                foreach(InventoryItem inventoryItem in inventory.ItemList)
                {
                    foreach(Item trophy in Trophies)
                    {
                        if (inventoryItem.Item.Id == trophy.Id)
                        {
                            questQty++;
                            continue;
                        }
                    }
                }
            }

            if (questQty >= Quantity)
            {
                CanComplete = true;
            }

            return CanComplete;
        }

        public override bool Complete(Adventurer adventurer)
        {
            if (!Validate(adventurer)) return false;

            Inventory inventory = adventurer.Character.Gear.Inventory;

            List<InventoryItem> itemList = new(inventory.ItemList);

            int questQty = 0;
            foreach(InventoryItem inventoryItem in itemList)
                {
                    foreach(Item trophy in Trophies)
                    {
                        if (inventoryItem.Item.Id == trophy.Id)
                        {
                            inventory.RemoveItem(inventoryItem);
                            questQty++;
                            continue;
                        }
                    }
                }

            if (questQty >= Quantity)
            {
                CanComplete = true;
            }

            return CanComplete;
        }
    }
}