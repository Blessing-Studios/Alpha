using Blessing.DataPersistence;

namespace Blessing.GameData
{
    public class PlayerData : Data
    {
        public new string Id;
        public string Name;        
        public PlayerData() : base()
        {
            Id = base.Id;
            Name = "";
        }
    }
}
