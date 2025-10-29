using System.Collections;
using System.Collections.Generic;
using Blessing.GameData;
using Blessing.Gameplay;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Ai
{
    public class SessionOwnerMultiSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item[] items;
        [SerializeField] private Gear[] gears;
        public List<AiCharacter> SpawnedAiCharacters;
        protected int m_TickToSpawnLoot;
        public int SpawnTime = 2;
        [Range(1.0f, 100.0f)] public float SpawnSize;

        void Awake()
        {
            GameManager.Singleton.MultiSpawner = this;
        }

        public override void Spawn()
        {
            GameManager.Singleton.AiCharacterSpawned++;

            Debug.Log(gameObject.name + ": Spawn");
            var spawnedNetworkObject = m_NetworkObjectToSpawn.InstantiateAndSpawn(NetworkManager.Singleton);

            // Handle Spawn position
            AiCharacter spawnedAiCharacter = spawnedNetworkObject.GetComponent<AiCharacter>();

            if (spawnedAiCharacter == null) Debug.LogError(gameObject.name + " AiCharacter is missing");

            spawnedAiCharacter.SpawnLocation = transform.position + new Vector3(Random.Range(-SpawnSize, SpawnSize), spawnedAiCharacter.CharacterController.height, Random.Range(-SpawnSize, SpawnSize));

            m_TickToSpawnLoot = SpawnTime;

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);

            spawnedAiCharacter.gameObject.name = m_NetworkObjectToSpawn.name;

            spawnedAiCharacter.gameObject.name += "-" + spawnedAiCharacter.GetComponent<NetworkObject>().GetHashCode();

            spawnedAiCharacter.Initialize();

            if (GameDataManager.Singleton.IsHost)
            {
                StartCoroutine(WaitToSpawnGear(spawnedAiCharacter));
            }

            SpawnedAiCharacters.Add(spawnedAiCharacter);
        }

        public void Spawn(int qty)
        {
            if (qty <= 0) return;

            StartCoroutine(WaitToSpawn(qty));
            
        }

        IEnumerator WaitToSpawn(int qty)
        {
            for (int i = 0; i < qty; i++)
            {
                Spawn();
                yield return null;
            } 
        }

        IEnumerator WaitToSpawnGear(AiCharacter spawnedAiCharacter)
        {
            yield return new WaitUntil(() => NetworkManager.Singleton.NetworkTickSystem.ServerTime.Tick > m_TickToSpawnLoot);

            // Spawn Gear
            foreach (Gear gear in gears)
            {
                InventoryItem inventoryItem = UIController.Singleton.CreateItem(gear);

                spawnedAiCharacter.Gear.AddEquipment(inventoryItem);
                inventoryItem.transform.SetParent(spawnedAiCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
                yield return null;
            }

            Inventory lootInventory = spawnedAiCharacter.Gear.Inventory;

            if (lootInventory == null) yield break;

            foreach (Item item in items)
            {
                InventoryItem inventoryItem = UIController.Singleton.CreateItem(item);

                lootInventory.AddItem(inventoryItem);
                inventoryItem.transform.SetParent(spawnedAiCharacter.transform, false);
                inventoryItem.gameObject.SetActive(false);
                yield return null;
            }

            StopCoroutine(WaitToSpawnGear(spawnedAiCharacter));
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(0,2.5f,0) , new Vector3(SpawnSize, 5f, SpawnSize));
        }
    }
}