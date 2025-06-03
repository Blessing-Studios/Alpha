using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Characters;
using UnityEngine;
using Blessing.Core.GameEventSystem;

namespace Blessing.Player
{

    public class PlayerCharacterMana : CharacterMana
    {
        [Header("Events")]
        public GameEvent OnWhiteManaChanged;
        public GameEvent OnRedManaChanged;
        public GameEvent OnGreenManaChanged;
        public GameEvent OnBlueManaChanged;
        public GameEvent OnBlackManaChanged;
        protected override void OnNetworkWhiteChanged(int previousValue, int newValue)
        {
            base.OnNetworkWhiteChanged(previousValue, newValue);

            // Raise Events
            if (OnWhiteManaChanged != null)
                OnWhiteManaChanged.Raise(this, newValue);
        }
        protected override void OnNetworkRedChanged(int previousValue, int newValue)
        {
            base.OnNetworkRedChanged(previousValue, newValue);

            // Raise Events
            if (OnRedManaChanged != null)
                OnRedManaChanged.Raise(this, newValue);
        }
        protected override void OnNetworkGreenChanged(int previousValue, int newValue)
        {
            base.OnNetworkGreenChanged(previousValue, newValue);

            // Raise Events
            if (OnGreenManaChanged != null)
                OnGreenManaChanged.Raise(this, newValue);
        }
        protected override void OnNetworkBlueChanged(int previousValue, int newValue)
        {
            base.OnNetworkBlueChanged(previousValue, newValue);

            // Raise Events
            if (OnBlueManaChanged != null)
                OnBlueManaChanged.Raise(this, newValue);
        }
        protected override void OnNetworkBlackChanged(int previousValue, int newValue)
        {
            base.OnNetworkBlackChanged(previousValue, newValue);

            // Raise Events
            if (OnBlackManaChanged != null)
                OnBlackManaChanged.Raise(this, newValue);
        }
    }
}
