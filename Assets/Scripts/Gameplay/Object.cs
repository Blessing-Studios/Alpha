using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Blessing.Gameplay
{
    class Object : NetworkTransform, ISpawnable
    {
        NetworkVariable<NetworkBehaviourReference> m_SessionOwnerNetworkObjectSpawner = new NetworkVariable<NetworkBehaviourReference>(writePerm: NetworkVariableWritePermission.Owner);
        NetworkVariable<bool> m_Initialized = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        Vector3 m_OriginalPosition;
        Quaternion m_OriginalRotation;
        protected bool canMove = true;
        protected Vector3 currentMovement = Vector3.zero;
        public float Gravity = -2.0f;
        public float GroundedGravity = -0.5f;
        public float distToGround = 0.1f;
        public Vector2 direction;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            gameObject.name = $"[NetworkObjectId-{NetworkObjectId}]{name}";
            m_OriginalPosition = transform.position;
            m_OriginalRotation = transform.rotation;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public void Init(SessionOwnerNetworkObjectSpawner spawner)
        {
            m_SessionOwnerNetworkObjectSpawner.Value = new NetworkBehaviourReference(spawner);
        }

        void Update()
        {
            HandleGravity();
            HandleMovement(canMove);
        }

        protected virtual void HandleGravity()
        {
            if (IsGrounded())
                currentMovement.y = GroundedGravity;
            else
                currentMovement.y += Gravity * Time.deltaTime;
        }

        public bool IsGrounded()
        {
            bool groundCheck = Physics.Raycast(transform.position, -Vector3.up, distToGround);
            return groundCheck;
        }

        protected virtual void HandleMovement(bool canMove)
        {
            if (!canMove)
            {
                currentMovement.x = 0.0f;
                currentMovement.z = 0.0f;
            }

            // Direction é útil para debugar
            direction.x = currentMovement.x;
            direction.y = currentMovement.z;
        }
    }
}
