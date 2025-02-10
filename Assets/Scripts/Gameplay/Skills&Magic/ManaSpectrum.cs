using System;
using UnityEngine;

namespace Blessing.Gameplay.SkillsAndMagic
{
    [Serializable]
    public class ManaSpectrum
    {
        // [SerializeField] public Mana White = new Mana(ManaColor.White);
        // [SerializeField] public Mana Blue;
        // [SerializeField] public Mana Green;
        // [SerializeField] public Mana Red;
        // [SerializeField] public Mana Black;

        [SerializeField] private Mana[] manas;

        public ManaSpectrum(Mana[] manas)
        {
            this.manas = manas;
        }

        public int Total 
        { 
            get 
            {
                int totalValue = 0;

                foreach ( Mana mana in manas )
                {
                    totalValue += mana.Value;
                }

                return totalValue; 
            }
        }

        public Mana GetMana(ManaColor color)
        {
            foreach ( Mana mana in manas )
            {
                if ( mana.Color == color )
                    return mana;
            }

            return manas[0];
        }

        public int GetValue(ManaColor color)
        {
            foreach ( Mana mana in manas )
            {
                if ( mana.Color == color )
                    return mana.Value;
            }

            return 0;
        }

        public void SetValue(ManaColor color, int value)
        {
            // Mana value can't be less than 0
            if (value < 0) value = 0;

            for ( int i = 0; i < manas.Length; i++)
            {
                if ( manas[i].Color == color)
                    manas[i].Value = value;
            }
        }

        public int GetRegen(ManaColor color)
        {
            foreach ( Mana mana in manas )
            {
                if ( mana.Color == color )
                    return mana.Regen;
            }

            return 0;
        }

        public void SetRegen(ManaColor color, int regen)
        {
            for ( int i = 0; i < manas.Length; i++)
            {
                if ( manas[i].Color == color)
                    manas[i].Regen = regen;
            }
        }
        public int GetDecay(ManaColor color)
        {
            foreach ( Mana mana in manas )
            {
                if ( mana.Color == color )
                    return mana.Decay;
            }

            return 0;
        }

        public void SetDecay(ManaColor color, int decay)
        {
            for ( int i = 0; i < manas.Length; i++)
            {
                if ( manas[i].Color == color)
                    manas[i].Decay = decay;
            }
        }
    }
}
