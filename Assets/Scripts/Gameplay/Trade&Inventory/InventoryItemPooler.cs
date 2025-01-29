using System;
using System.Collections.Generic;
using Blessing.Core.ObjectPooling;
using UnityEngine;

namespace Blessing.Gameplay.TradeAndInventory
{
    public class InventoryItemPooler : AbstractObjectPooler<InventoryItem>
    {
        public GameObject InventoryItemPrefab { get { return GameManager.Singleton.InventoryItemPrefab; } }

        protected override InventoryItem CreateObject()
        {
            InventoryItem inventoryItem = Instantiate(InventoryItemPrefab).GetComponent<InventoryItem>();
            inventoryItem.Pool = Pool;

            return inventoryItem;
        }
        protected override void OnGetFromPool(InventoryItem pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        protected override void OnReleaseToPool(InventoryItem pooledObject)
        {
            pooledObject.gameObject.SetActive(false);

            pooledObject.transform.SetParent(transform, false);
        }
        protected override void OnDestroyPooledObject(InventoryItem pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}

