using System;
using System.Collections;
using Blessing.GameData;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay
{
    class SessionOwnerLootSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item[] items;
        private Inventory lootInventory;
        protected int m_TickToSpawnLoot;
        public int SpawnTime = 2;
        public override void Spawn()
        {
            var spawnedNetworkObject = Instantiate(m_NetworkObjectToSpawn, position: transform.position, rotation: transform.rotation);

            spawnedNetworkObject.Spawn();

            m_TickToSpawnLoot = SpawnTime;

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);

            lootInventory = spawnedNetworkObject.GetComponent<Inventory>();

            if (GameDataManager.Singleton.IsHost)
                StartCoroutine(WaitToSpawnLoot());
        }
        IEnumerator WaitToSpawnLoot()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot);

            SpawnLoot();
        
            StopCoroutine(WaitToSpawnLoot());
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void SpawnLoot()
        {
            foreach(Item item in items)
            {
                InventoryItem inventoryItem = GameManager.Singleton.GetInventoryItem();
                inventoryItem.Set(item);

                lootInventory.AddItem(inventoryItem);
                inventoryItem.transform.SetParent(lootInventory.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }
        }
    }
}

