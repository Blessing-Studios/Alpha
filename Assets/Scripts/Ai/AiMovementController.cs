using UnityEngine;

namespace Blessing.Ai
{
    public class AiMovementController : MovementController
    {
        public void HandleAiMovement(Vector2 currentMovementInput)
        {
            if (!HasAuthority) return;
            
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

            if (isMovementPressed)
            {
                animator.SetBool(isWalkingHash, true);
            }
            else
            {
                animator.SetBool(isWalkingHash, false);
            }
        }
    }
}
