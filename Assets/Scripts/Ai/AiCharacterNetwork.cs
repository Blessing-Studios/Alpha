using Blessing.Gameplay;
using Blessing.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;


namespace Blessing.Ai
{
    public class AiCharacterNetwork : CharacterNetwork, ISpawnable
    {
        NetworkVariable<NetworkBehaviourReference> m_SessionOwnerNetworkObjectSpawner = new NetworkVariable<NetworkBehaviourReference>(writePerm: NetworkVariableWritePermission.Owner);

        public void Init(SessionOwnerNetworkObjectSpawner spawner)
        {
            
        }
    }
}
