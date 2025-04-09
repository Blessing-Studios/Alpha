using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blessing.Gameplay.Interation
{
    public interface IInteractable
    {
        public string name { get; }
        public Transform transform { get; }
        public void Interact(Interactor interactor);
        public bool CanInteract { get;}
    }
}
