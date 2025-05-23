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

            Debug.Log(gameObject.name + ": Spawn -" + m_NetworkObjectToSpawn.name);
            spawnedNetworkObject.Spawn();

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);
        }
    }
}

