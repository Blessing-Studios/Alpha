using System;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public enum ManaColor { White, Blue, Green, Red, Black } 

    [Serializable]
    public struct Mana
    {
        public ManaColor Color;
        public int Value;
        public int Regen;
        public int Decay; // Value of mana that change over time, can be negative

        public Mana( ManaColor color, int value = 0, int regen = 0, int decay = 0 )
        {
            Color = color;
            Value = value;
            Regen = regen;
            Decay = decay;
        }
    }
}