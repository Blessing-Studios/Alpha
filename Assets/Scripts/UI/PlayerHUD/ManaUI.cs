using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.PlayerHUD
{
    public class ManaUI : MonoBehaviour
    {
        public Image Icon;
        public TextMeshProUGUI ManaValueText;
        public void Initialize()
        {

        }

        public void UpdateValue(int value)
        {
            ManaValueText.text = value.ToString();
        }
    }
}
