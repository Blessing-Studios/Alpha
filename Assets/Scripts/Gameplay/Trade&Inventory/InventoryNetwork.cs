using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    // Não está sendo usado
    public class InventoryNetwork : NetworkBehaviour
    {
        public Inventory Inventory;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        
        public NetworkList<int> InventoryIdList;
        public List<int> InventoryIds = new();

        void Awake()
        {
            InventoryIdList = new NetworkList<int>(new List<int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        }    

        public void OnAddItem(Component sender, object itemId)
        {
            if (itemId is not int) return;
            int id = (int) itemId;

            InventoryIdList.Add(id);
            InventoryIds.Add(id);
        }

        public void OnRemoveItem(Component sender, object itemId)
        {
            if (itemId is not int) return;
            int id = (int) itemId;
            
            InventoryIdList.Remove(id);
            InventoryIds.Remove(id);
        }
    }
}

