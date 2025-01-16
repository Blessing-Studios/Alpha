using System;
using Blessing.Characters;
using Blessing.Gameplay.Characters;
using TMPro;
using UnityEngine;


namespace Blessing.Gameplay.TradeAndInventory
{
    public class CharacterStatsInfo : MonoBehaviour
    {
        public CharacterStats CharacterStats;
        public TextMeshProUGUI StrengthText;
        public TextMeshProUGUI ConstitutionText;
        public TextMeshProUGUI DexterityText;
        public TextMeshProUGUI IntelligenceText;
        public TextMeshProUGUI WisdomText;
        public TextMeshProUGUI CharismaText;
        public TextMeshProUGUI LuckText;

        public void Initialize()
        {
            UpdateStatInfo();
        }

        public void UpdateStatInfo()
        {
            if (CharacterStats == null)
            {
                Debug.Log(gameObject.name + " CharacterStats is missing for non player characters");
                return;
            }

            StrengthText.text = "Strength: " + CharacterStats.Strength;
            ConstitutionText.text = "Constitution: " + CharacterStats.Constitution;
            DexterityText.text = "Dexterity: " + CharacterStats.Dexterity;
            IntelligenceText.text = "Intelligence: " + CharacterStats.Intelligence;
            WisdomText.text = "Wisdom: " + CharacterStats.Wisdom;
            CharismaText.text = "Charisma: " + CharacterStats.Charisma;
            LuckText.text = "Luck: " + CharacterStats.Luck;
        }
    }
}
