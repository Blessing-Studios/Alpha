using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay
{
    interface ISpawnable
    {
        NetworkObject NetworkObject
        {
            get;
        }

        void Init(SessionOwnerNetworkObjectSpawner networkObjectSpawner);
    }
}
