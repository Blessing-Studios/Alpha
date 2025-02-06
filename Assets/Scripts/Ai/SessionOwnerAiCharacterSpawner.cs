using System.Collections;
using Blessing.Gameplay;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Ai
{
    class SessionOwnerAiCharacterSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item[] items;
        [SerializeField] private Gear[] gears;

        AiCharacter spawnedAiCharacter;
        protected NetworkVariable<int> m_TickToSpawnLoot = new NetworkVariable<int>();
        public int SpawnTime = 2;
        public override void Spawn()
        {
            var spawnedNetworkObject = m_NetworkObjectToSpawn.InstantiateAndSpawn(NetworkManager, ownerClientId: OwnerClientId);
            
            // Handle Spawn position
            spawnedAiCharacter = spawnedNetworkObject.GetComponent<AiCharacter>();

            if (spawnedAiCharacter == null) Debug.LogError(gameObject.name + " AiCharacter is missing");

            spawnedAiCharacter.SpawnLocation = transform.position;

            m_TickToSpawnLoot.Value = SpawnTime;

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);
            m_IsRespawning.Value = false;

            if (HasAuthority)
            {
                StartCoroutine(WaitToSpawnGear());
            }
                
        }
        IEnumerator WaitToSpawnGear()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot.Value);

            SpawnGear();

            StopCoroutine(WaitToSpawnGear());
        }

        private void SpawnGear()
        {
            if (!HasAuthority) return;

            foreach (Gear gear in gears)
            {
                InventoryItem inventoryItem = GameManager.Singleton.GetInventoryItem();
                inventoryItem.Set(gear);

                spawnedAiCharacter.Gear.AddEquipment(inventoryItem);
                inventoryItem.transform.SetParent(spawnedAiCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }

            Inventory lootInventory = spawnedAiCharacter.Gear.Inventory;
            if (lootInventory == null) return;

            foreach (Item item in items)
            {
                InventoryItem inventoryItem = GameManager.Singleton.GetInventoryItem();
                inventoryItem.Set(item);

                lootInventory.AddItem(inventoryItem);
                inventoryItem.transform.SetParent(spawnedAiCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
            }
        }
    }
}