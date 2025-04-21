using Blessing.Ai;
using UnityEngine;

namespace Blessing.AI.Goap
{
    public interface ICharacterAgent
    {
        public AiCharacter AiCharacter { get; }
        public Vector3 MinRange { get; }
    }
}
