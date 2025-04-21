using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace Blessing.UI.CharacterSelection
{
    [RequireComponent(typeof(Button))]
    public class ArchetypeElement : MonoBehaviour
    {
        public Image Icon;
        public TextMeshProUGUI Name;
        [HideInInspector] public Button Button;

        void Awake()
        {
            Button = GetComponent<Button>();
        }
    }
}
