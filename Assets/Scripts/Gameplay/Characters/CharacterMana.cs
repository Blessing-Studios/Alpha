using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.SkillsAndMagic;
using Unity.Netcode;
using Blessing.Gameplay.Characters.Traits;

namespace Blessing.Gameplay.Characters
{
    public class CharacterMana : NetworkBehaviour
    {
        // Tentar criar a classe sem usar uma MonoBehaviour
        // public event EventHandler OnHealthChanged;
        protected IEnumerator coroutine;
        [SerializeField] protected ManaSpectrum manaReserve;
        [SerializeField] ManaSpectrum reserveMax;
        [SerializeField] protected const int statsMultiplier = 10;
        [Tooltip("Base Mana Regenerated by time")][SerializeField] protected int manaChange = 2;
        [SerializeField] protected float manaChangeTime = 0.5f;
        public float ChangeTime { get { return manaChangeTime; } }

        [field: SerializeField]
        protected NetworkVariable<int> white = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int White { get { return white.Value; } protected set { white.Value = value; } }
        [field: SerializeField]
        protected NetworkVariable<int> red = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int Red { get { return red.Value; } protected set { red.Value = value; } }
        [field: SerializeField]
        protected NetworkVariable<int> green = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int Green { get { return green.Value; } protected set { green.Value = value; } }
        [field: SerializeField]
        protected NetworkVariable<int> blue = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int Blue { get { return blue.Value; } protected set { blue.Value = value; } }
        [field: SerializeField]
        protected NetworkVariable<int> black = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int Black { get { return black.Value; } protected set { black.Value = value; } }

        // Start is called before the first frame update
        void Awake()
        {
            InitializeManaReserve();
        }

        private void InitializeManaReserve()
        {
            // manaChange is the initial mana regen of the character
            // By default, mana decay will be half the mana regen
            // reserveMax will salve the value of the original manaChange if needed
            manaReserve = new ManaSpectrum(new Mana[]
            {
                    new(ManaColor.White, 50, manaChange, manaChange / 2),
                    new(ManaColor.Red, 50, manaChange, manaChange / 2),
                    new(ManaColor.Green, 50, manaChange, manaChange / 2),
                    new(ManaColor.Blue, 50, manaChange, manaChange / 2),
                    new(ManaColor.Black, 50, manaChange, manaChange / 2)
            });

            reserveMax = new ManaSpectrum(new Mana[]
            {
                    new(ManaColor.White, 100, manaChange, manaChange / 2),
                    new(ManaColor.Red, 100, manaChange, manaChange / 2),
                    new(ManaColor.Green, 100, manaChange, manaChange / 2),
                    new(ManaColor.Blue, 100, manaChange, manaChange / 2),
                    new(ManaColor.Black, 100, manaChange, manaChange / 2)
            });
        }

        public void SetManaParameters(CharacterStats stats, List<CharacterTrait> traits)
        {
            // Set Max Values

            // White Mana Max, TODO:
            reserveMax.SetValue(ManaColor.White, 100);

            // Red Mana Max value comes from Strength
            reserveMax.SetValue(ManaColor.Red, stats.Strength * statsMultiplier);

            // Green Mana Max value comes from Constitution
            reserveMax.SetValue(ManaColor.Green, stats.Constitution * statsMultiplier);

            // Blue Mana Max value comes from Wisdom
            reserveMax.SetValue(ManaColor.Blue, stats.Wisdom * statsMultiplier);

            // Black Mana Max, TODO:
            reserveMax.SetValue(ManaColor.Black, 100);

            // Time to regenerate mana comes from total white mana, more white mana, faster mana regen

            manaChangeTime = 5.0f / ((100.0f + 9 * manaReserve.GetValue(ManaColor.White)) / 100.0f);

            // if (manaReserve.GetValue(ManaColor.White) == 0)
            //     manaChangeTime = 50;
            // else
            //     manaChangeTime = 50 / manaReserve.GetValue(ManaColor.White);


            // Mana gain by each mana regen cycle
            // manaChangeTime = 2

            // Apply Traits
            foreach (ManaColor manaColor in Enum.GetValues(typeof(ManaColor)))
            {
                int manaRegen = manaChange;
                int manaDecay = manaChange / 2;
                foreach (CharacterTrait characterTrait in traits)
                {
                    manaRegen += characterTrait.Trait.GetManaRegen(manaColor);
                    manaDecay += characterTrait.Trait.GetManaDecay(manaColor);
                }
                manaReserve.SetRegen(manaColor, manaRegen);
                manaReserve.SetDecay(manaColor, manaDecay);
            }
        }

        public int GetMaxMana(ManaColor manaColor)
        {
            return reserveMax.GetValue(manaColor);
        }

        public int GetManaValue(ManaColor manaColor)
        {
            return manaReserve.GetValue(manaColor);
        }

        public override void OnNetworkSpawn()
        {
            white.OnValueChanged += OnNetworkWhiteChanged;
            red.OnValueChanged += OnNetworkRedChanged;
            green.OnValueChanged += OnNetworkGreenChanged;
            blue.OnValueChanged += OnNetworkBlueChanged;
            black.OnValueChanged += OnNetworkBlackChanged;
        }
        public override void OnNetworkDespawn()
        {
            white.OnValueChanged -= OnNetworkWhiteChanged;
            red.OnValueChanged -= OnNetworkRedChanged;
            green.OnValueChanged -= OnNetworkGreenChanged;
            blue.OnValueChanged -= OnNetworkBlueChanged;
            black.OnValueChanged -= OnNetworkBlackChanged;
        }
        protected virtual void OnNetworkWhiteChanged(int previousValue, int newValue)
        {
            manaReserve.SetValue(ManaColor.White, newValue);
        }
        protected virtual void OnNetworkRedChanged(int previousValue, int newValue)
        {
            manaReserve.SetValue(ManaColor.Red, newValue);
        }
        protected virtual void OnNetworkGreenChanged(int previousValue, int newValue)
        {
            manaReserve.SetValue(ManaColor.Green, newValue);
        }
        protected virtual void OnNetworkBlueChanged(int previousValue, int newValue)
        {
            manaReserve.SetValue(ManaColor.Blue, newValue);
        }
        protected virtual void OnNetworkBlackChanged(int previousValue, int newValue)
        {
            manaReserve.SetValue(ManaColor.Black, newValue);
        }

        public void SpendMana(ManaColor color, int manaAmount)
        {
            if (manaAmount <= 0) return;

            int manaValue = manaReserve.GetValue(color);

            manaValue -= manaAmount;

            // Don't let health be negative
            if (manaValue < 0) manaValue = 0;

            manaReserve.SetValue(color, manaValue);
        }

        public bool SpendManaSpectrum(ManaSpectrum manaSpectrumCost)
        {
            //Check if has enough mana to spend in manaReserve
            if (!HasEnoughMana(manaSpectrumCost)) return false;

            // Spent mana
            foreach (ManaColor manaColor in Enum.GetValues(typeof(ManaColor)))
            {
                int newValue = manaReserve.GetValue(manaColor) - manaSpectrumCost.GetValue(manaColor);

                manaReserve.SetValue(manaColor, newValue);
            }

            return true;
        }

        public bool HasEnoughMana(ManaSpectrum manaSpectrumCost)
        {
            bool hasEnoughMana = true;
            foreach (ManaColor manaColor in Enum.GetValues(typeof(ManaColor)))
            {
                if (manaReserve.GetValue(manaColor) < manaSpectrumCost.GetValue(manaColor))
                    hasEnoughMana = false;
            }

            return hasEnoughMana;
        }

        public void ReceiveMana(ManaColor color, int manaAmount)
        {
            if (manaAmount <= 0) return;

            int newValue = manaReserve.GetValue(color) + manaAmount;

            // Check for max color mana value
            if (reserveMax.GetValue(color) < newValue)
                newValue = reserveMax.GetValue(color);

            manaReserve.SetValue(color, newValue);
        }

        public void ChangeByTime()
        {
            foreach (ManaColor manaColor in Enum.GetValues(typeof(ManaColor)))
            {
                int manaDiff = manaReserve.GetValue(manaColor) - reserveMax.GetValue(manaColor);

                // Current Value is less than max, should regen mana
                if (manaDiff < 0)
                    ReceiveMana(manaColor, manaReserve.GetRegen(manaColor));

                // Current Value is more than max, mana will decay
                if (manaDiff > 0)
                    SpendMana(manaColor, manaReserve.GetDecay(manaColor));

                // Update Mana Field Value
                GetType().GetProperty(manaColor.ToString()).SetValue(this, manaReserve.GetValue(manaColor));

                // UpdateManas(manaColor);
            }
        }

        private void UpdateManas(ManaColor manaColor)
        {
            switch(manaColor)
            {
                case ManaColor.White: White = manaReserve.GetValue(manaColor);  break;
                case ManaColor.Red: Red = manaReserve.GetValue(manaColor);  break;
                case ManaColor.Green: Green = manaReserve.GetValue(manaColor);  break;
                case ManaColor.Blue: Blue = manaReserve.GetValue(manaColor);  break;
                case ManaColor.Black: Black = manaReserve.GetValue(manaColor);  break;
                default: ; break;
            }
        }
    }
}
