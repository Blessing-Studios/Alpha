using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Blessing.UI.CharacterSelection
{
    [RequireComponent(typeof(Button))]
    public class CharacterSlot : MonoBehaviour
    {
        public Image Icon;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI RankAndClass;
    }
}
