using System;
using System.Collections;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay
{
    class SessionOwnerLootSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item[] items;
        private Inventory lootInventory;
        protected NetworkVariable<int> m_TickToSpawnLoot = new NetworkVariable<int>();
        public int SpawnTime = 2;
        public override void Spawn()
        {
            var spawnedNetworkObject = Instantiate(m_NetworkObjectToSpawn, position: transform.position, rotation: transform.rotation);

            spawnedNetworkObject.Spawn();

            m_TickToSpawnLoot.Value = SpawnTime;

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);
            m_IsRespawning.Value = false;

            lootInventory = spawnedNetworkObject.GetComponent<Inventory>();

            if (HasAuthority)
                StartCoroutine(WaitToSpawnLoot());
        }
        IEnumerator WaitToSpawnLoot()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot.Value);

            SpawnLoot();
        
            StopCoroutine(WaitToSpawnLoot());
        }

        private void SpawnLoot()
        {
            if (!HasAuthority) return;

            foreach(Item item in items)
            {
                InventoryItem inventoryItem = GameManager.Singleton.GetInventoryItem();
                inventoryItem.Set(item);

                lootInventory.AddItem(inventoryItem);
            }
        }
    }
}

