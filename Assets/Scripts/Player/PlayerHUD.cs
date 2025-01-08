using TMPro;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerHUD : MonoBehaviour
    {
        public TextMeshProUGUI GoldText;

        [field: SerializeField] public PlayerController PlayerController { get; private set; }
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }

        void Awake()
        {
            PlayerController = GetComponent<PlayerController>();
        }

        void Start()
        {
            if (GoldText == null)
            {
                Debug.LogError(gameObject.name + " GoldText is missing");
            }

            PlayerCharacter = PlayerController.PlayerCharacter;

            if (PlayerCharacter == null)
                GoldText.gameObject.SetActive(false);
        }
        
        void Update()
        {
            HandleGoldText();
        }

        private void HandleGoldText()
        {
            if (GoldText == null) return;

            if (PlayerCharacter == null) return;

            GoldText.text = "Gold " + PlayerCharacter.Gear.Gold;
        }
    }
}

