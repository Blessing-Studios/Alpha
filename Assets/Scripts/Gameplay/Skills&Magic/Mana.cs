using System;

namespace Blessing.Gameplay.SkillsAndMagic
{
    public enum ManaColor { White, Blue, Green, Red, Black } 

    [Serializable]
    public struct Mana
    {
        public ManaColor Color;
        public int Value;

        public Mana( ManaColor color, int value = 0 )
        {
            Color = color;
            Value = value;
        }
    }
}