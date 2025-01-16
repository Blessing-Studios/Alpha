using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blessing.Gameplay.Interation
{
    interface IInteractable
    {
        public Transform transform { get;}
        public void Interact(Interactor interactor);
    }

    public class Interactor : MonoBehaviour
    {
        public Transform InteractorSource;
        public float InteractRange = 2f;
        private List<IInteractable> interactables = new();
        [SerializeField] private IInteractable currentInteracting;
        public float MaxDistance = 5;

        public void HandleInteraction()
        {
            interactables = new();

            Collider[] hitColliders = Physics.OverlapSphere(InteractorSource.position, InteractRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.TryGetComponent(out IInteractable other))
                {
                    // Can't interact with itself
                    if (transform == other.transform) continue;

                    interactables.Add(other);
                }
            }

            IInteractable closest = GetClosest(InteractorSource.position, interactables);

            if (closest != null)
            {
                currentInteracting = closest;
                currentInteracting.Interact(this);
            }
        }

        private IInteractable GetClosest(Vector3 startPosition, List<IInteractable> interactables)
        {
            IInteractable bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (IInteractable potentialTarget in interactables )
            {
                Vector3 directionToTarget = potentialTarget.transform.position - startPosition;

                float dSqrToTarget = directionToTarget.sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }
        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(InteractorSource.position, InteractRange);
        }
    }
}

