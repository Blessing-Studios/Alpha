using UnityEngine;
using Unity.Netcode;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    public abstract class Character : NetworkBehaviour
    {
        public float AttackPressedTimerWindow = 0.2f;
        protected MovementController movementController;
        public int GetCurrentHealth()
        {
            return 100;
        }

        public MovementController GetMovementController()
        {
            return movementController;
        }
        public abstract bool CheckIfAttackPressed(string nextComboAction);
    }
}