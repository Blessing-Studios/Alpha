using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blessing.Gameplay.Interation
{
    public class Interactor : MonoBehaviour
    {
        public Transform InteractorSource;
        public Vector3 SourceOffset = Vector3.zero;
        public float InteractRange = 2f;
        private List<IInteractable> interactables = new();
        [SerializeField] private IInteractable currentInteracting;
        public float MaxDistance = 5;

        public void HandleInteraction()
        {
            interactables = new();

            Collider[] hitColliders = Physics.OverlapSphere(InteractorSource.position + SourceOffset, InteractRange);
            foreach (var hitCollider in hitColliders)
            {
                // Can't interact with itself
                if (transform == hitCollider.transform) continue;

                IInteractable[] result;
                result = hitCollider.gameObject.GetComponents<IInteractable>();

                foreach (IInteractable interactable in result)
                {
                    if (interactable.CanInteract == true)
                        interactables.Add(interactable);
                }
            }

            if (interactables.Count > 1)
            {
                GameManager.Singleton.ContextDropDownMenu.AddInteractables(interactables, this);
            }
            else if (interactables.Count == 1)
            {
                interactables[0].Interact(this);
            }

            // IInteractable closest = GetClosest(InteractorSource.position, interactables);

            // if (closest != null)
            // {
            //     currentInteracting = closest;
            //     currentInteracting.Interact(this);
            // }
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
            Gizmos.DrawWireSphere(InteractorSource.position + SourceOffset, InteractRange);
        }
    }
}

