using System;
using UnityEngine;

namespace Blessing.Gameplay.Guild.Quests
{
    public enum ObjectiveType { Kill, Delivery }
    public abstract class Objective
    {
        public abstract ObjectiveType Type { get; }
        public bool IsCompleted = false;
        public bool CanComplete = false;

        public abstract bool Validate(Adventurer adventurer);
        public abstract bool Complete(Adventurer adventurer);
    }
}
