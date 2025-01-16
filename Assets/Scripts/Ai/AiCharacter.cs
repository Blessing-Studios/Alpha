using Blessing.Gameplay.Characters;
using UnityEngine;

namespace Blessing.Ai
{
    public class AiCharacter : Character
    {
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
        public override bool CheckIfActionTriggered(string actionName)
        {
            return true;
        }
    }
}
