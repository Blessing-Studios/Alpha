using TMPro;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerHUD : MonoBehaviour
    {
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI GoldText;
        [field: SerializeField] public PlayerController PlayerController { get; private set; }
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }

        void Awake()
        {
            PlayerController = GetComponent<PlayerController>();
        }

        void Start()
        {
            if (HpText == null)
            {
                Debug.LogError(gameObject.name + " HpText is missing");
            }
            if (GoldText == null)
            {
                Debug.LogError(gameObject.name + " GoldText is missing");
            }

            PlayerCharacter = PlayerController.PlayerCharacter;

            if (PlayerCharacter == null)
            {
                GoldText.gameObject.SetActive(false);
                HpText.gameObject.SetActive(false);
            }
        }
        
        void FixedUpdate()
        {
            HandleGoldText();
            HandleHpText();
        }
        private void HandleHpText()
        {
            if (HpText == null) return;

            if (PlayerCharacter == null) return;

            HpText.text = "HP " + PlayerCharacter.Health.GetHealth();
        }
        private void HandleGoldText()
        {
            if (GoldText == null) return;

            if (PlayerCharacter == null) return;

            GoldText.text = "Gold " + PlayerCharacter.Gear.Gold;
        }
    }
}

