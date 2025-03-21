using System;
using System.Collections.Generic;
using Blessing.DataPersistence;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;

namespace Blessing.GameData
{
    
    [Serializable]
    public class CharacterData : IEquatable<CharacterData>, INetworkSerializeByMemcpy
    {
        public string Id;
        public string Name;
        public int ArchetypeId;
        public List<InventoryItemData> Gears;
        public List<InventoryItemData> Items;

        public CharacterData(string name = "", int archetypeId = 1)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            ArchetypeId = archetypeId;
        }

        public bool Equals(CharacterData other)
        {
            return
                (this.Id == other.Id) &&
                (this.Name == other.Name);
        }
    }
    public class PlayerData : Data
    {
        public new string Id;
        public string Name;
        public List<CharacterData> Characters;        
        public PlayerData() : base()
        {
            Id = base.Id;
            Name = "";
            Characters = new();
        }
    }
}
