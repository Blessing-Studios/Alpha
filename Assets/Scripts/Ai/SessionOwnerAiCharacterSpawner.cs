using System.Collections;
using Blessing.GameData;
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
        protected int m_TickToSpawnLoot;
        public int SpawnTime = 2;

        public override void Spawn()
        {
            GameManager.Singleton.AiCharacterSpawned ++;

            Debug.Log(gameObject.name + ": Spawn");
            var spawnedNetworkObject = m_NetworkObjectToSpawn.InstantiateAndSpawn(NetworkManager.Singleton);
            
            // Handle Spawn position
            spawnedAiCharacter = spawnedNetworkObject.GetComponent<AiCharacter>();

            if (spawnedAiCharacter == null) Debug.LogError(gameObject.name + " AiCharacter is missing");

            spawnedAiCharacter.SpawnLocation = transform.position + new Vector3(0, spawnedAiCharacter.CharacterController.height, 0);

            m_TickToSpawnLoot = SpawnTime;

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);

            spawnedAiCharacter.gameObject.name = m_NetworkObjectToSpawn.name;

            spawnedAiCharacter.gameObject.name += "-" + spawnedAiCharacter.GetComponent<NetworkObject>().GetHashCode();

            spawnedAiCharacter.Initialize();

            if (GameDataManager.Singleton.IsHost)
            {
                StartCoroutine(WaitToSpawnGear());
            }
                
        }
        IEnumerator WaitToSpawnGear()
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot);

            SpawnGear();

            StopCoroutine(WaitToSpawnGear());
        }

        private void SpawnGear()
        {
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