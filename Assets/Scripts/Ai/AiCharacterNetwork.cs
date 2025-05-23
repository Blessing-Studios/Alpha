using Blessing.Gameplay;
using Blessing.Gameplay.Characters;
using Unity.Netcode;
using UnityEngine;


namespace Blessing.Ai
{
    public class AiCharacterNetwork : CharacterNetwork, ISpawnable
    {
        NetworkVariable<NetworkBehaviourReference> m_SessionOwnerNetworkObjectSpawner = new NetworkVariable<NetworkBehaviourReference>(writePerm: NetworkVariableWritePermission.Owner);
        protected AiCharacter aiCharacter;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            aiCharacter = GetComponent<AiCharacter>();
        }
        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);

            // When change Ownership, Enable/Diable components
            if (HasAuthority)
            {
                aiCharacter.EnableNavMashAgent(true);
            }
            else
            {
                aiCharacter.EnableNavMashAgent(false);
            }
        }
        public void Init(SessionOwnerNetworkObjectSpawner spawner)
        {
            
        }
    }
}
