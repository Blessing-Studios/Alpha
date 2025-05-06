using System;
using System.Collections.Generic;
using Blessing.Gameplay.TradeAndInventory;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

namespace Blessing.Gameplay.Guild.Quests
{
    [Serializable]
    public class KillObjective : Objective
    {
        public override ObjectiveType Type { get {return ObjectiveType.Kill; } }
        public Item[] Trophies;
        public int Quantity;
        public override string Description { get { return GetDescription(); }}
        public KillObjective(Item[] trophies, int quantity)
        {
            Trophies = trophies;
            Quantity = quantity;
        }
        public string GetDescription()
        {
            if (Trophies.Length == 0) return "Kill";

            string itemsName = Trophies[0].Label;

            if (Trophies.Length > 1)
            for (int i = 1; i < Trophies.Length; i++)
            {
                itemsName += " or " + Trophies[i].Label;
            }

            return $"Bring {Quantity} of the following item: {itemsName}";
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