using System;
using UnityEngine;

namespace Blessing.Gameplay.Characters
{
    [Serializable]
    public struct CharacterHitAudio
    {
        public HitType HitType;
        public AudioClip[] Clips;
    }
}
