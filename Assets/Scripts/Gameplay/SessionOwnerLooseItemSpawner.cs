using System.Collections;
using Blessing.Gameplay.TradeAndInventory;
using UnityEngine;

namespace Blessing.Gameplay
{
    class SessionOwnerLooseItemSpawner : SessionOwnerNetworkObjectSpawner
    {
        [SerializeField] private Item item;
        [SerializeField] private int stack = 1;
        public override void Spawn()
        {
            var spawnedNetworkObject = Instantiate(m_NetworkObjectToSpawn, position: transform.position, rotation: transform.rotation);

            LooseItem looseItem = spawnedNetworkObject.GetComponent<LooseItem>();
            looseItem.Item = item;
            looseItem.Stack = stack;

            Debug.Log(gameObject.name + ": Spawn -" + m_NetworkObjectToSpawn.name);
            spawnedNetworkObject.Spawn();

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();


            spawnable.Init(this);
        }
    }
}

