using Blessing.Gameplay.TradeAndInventory;
using UnityEngine;

namespace Blessing.Gameplay
{
    class SessionOwnerLooseItemSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item item;
        public override void Spawn()
        {
            var spawnedNetworkObject = Instantiate(m_NetworkObjectToSpawn, position: transform.position, rotation: transform.rotation);

            spawnedNetworkObject.GetComponent<LooseItem>().Item = item;
            
            spawnedNetworkObject.Spawn();

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);
            m_IsRespawning.Value = false;
        }
    }
}

