using System.Collections;
using Blessing.GameData;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing.Gameplay
{
    public class SessionOwnerNetworkObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        protected NetworkObject m_NetworkObjectToSpawn;

        void Awake()
        {
            GameManager.Singleton.ObjectSpawners.Add(this);
        }

        void Start()
        {
            // Debug.Log(gameObject.name + ": Start");

            // if (GameDataManager.Singleton.IsHost)
            // {
            //     Debug.Log(gameObject.name + ": Start - IsHost");
            //     Spawn();
            // }
        }

        public virtual void Spawn()
        {
            var spawnedNetworkObject = m_NetworkObjectToSpawn.InstantiateAndSpawn(NetworkManager.Singleton, position: transform.position, rotation: transform.rotation);

            var spawnable = spawnedNetworkObject.GetComponent<ISpawnable>();
            spawnable.Init(this);
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
