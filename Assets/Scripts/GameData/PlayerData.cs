using System.Collections.Generic;
using Blessing.DataPersistence;
using Blessing.Gameplay.TradeAndInventory;

namespace Blessing.GameData
{
    public class PlayerData : Data
    {
        public new string Id;
        public string Name;
        public List<InventoryItemData> Gears;
        public List<InventoryItemData> Items;        
        public PlayerData() : base()
        {
            Id = base.Id;
            Name = "";
        }
    }
}
