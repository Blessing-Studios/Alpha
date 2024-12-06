using UnityEngine;

namespace Blessing.Player
{
    public class PlayerSpawn : MonoBehaviour
    {
        void Awake()
        {
            GameManager.Singleton.AddPlayerSpawn(this.transform);
        }
        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, new Vector3(0.848f, 0.501f, 0.694f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
