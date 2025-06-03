using Blessing.Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.PlayerHUD
{
    public class AbilityUI : MonoBehaviour
    {
        public CharacterAbility Ability;
        public Image Icon;
        public void Initialize(CharacterAbility ability)
        {
            Ability = ability;

            Icon.sprite = ability.IconSprite;
        }
    }
}
