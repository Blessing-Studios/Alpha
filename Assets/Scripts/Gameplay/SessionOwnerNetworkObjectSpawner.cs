using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay
{
    public class SessionOwnerNetworkObjectSpawner : NetworkBehaviour
    {
        [SerializeField]
        protected NetworkObject m_NetworkObjectToSpawn;
        protected NetworkVariable<bool> m_IsRespawning = new NetworkVariable<bool>();

        protected NetworkVariable<int> m_TickToRespawn = new NetworkVariable<int>();

        public override void OnNetworkSpawn()
        {
            if (HasAuthority)
            {
                Spawn();
            }
        }

        public override void OnNetworkDespawn()
        {
            StopAllCoroutines();
        }

        public virtual void Spawn()
        {
            var spawnedNetworkObject = m_NetworkObjectToSpawn.InstantiateAndSpawn(NetworkManager, position: transform.position, rotation: transform.rotation);

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);

            m_IsRespawning.Value = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="respawnTime"> Network tick at which to respawn this NetworkObject prefab </param>
        [Rpc(SendTo.Authority)]
        public void RespawnRpc(int respawnTime)
        {
            m_TickToRespawn.Value = respawnTime;
            m_IsRespawning.Value = true;
            StartCoroutine(WaitToRespawn());
        }

        IEnumerator WaitToRespawn()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkTickSystem.ServerTime.Tick > m_TickToRespawn.Value);
            Spawn();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            if (HasAuthority && m_IsRespawning.Value)
            {
                StartCoroutine(WaitToRespawn());
            }
            else
            {
                StopAllCoroutines();
            }
        }

        // Add gizmo to show the spawn position of the network object
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(0.848f, 0.501f, 0.694f));
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
