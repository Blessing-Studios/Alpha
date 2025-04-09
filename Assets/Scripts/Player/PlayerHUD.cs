using Blessing.HealthAndDamage;
using TMPro;
using UnityEngine;

namespace Blessing.Player
{
    public class PlayerHUD : MonoBehaviour
    {
        public SegmentedHealthBar HealthBar;
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI GoldText;
        public Canvas PlayerCanvas;
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

            if (PlayerCanvas == null)
            {
                Debug.LogError(gameObject.name + " PlayerCanvas is missing");
            }

            if (PlayerCharacter == null)
            {
                GoldText.gameObject.SetActive(false);
                HpText.gameObject.SetActive(false);
                PlayerCanvas.gameObject.SetActive(false);
            }
        }
        public void Initialize(PlayerCharacter playerCharacter)
        {
            PlayerCharacter = playerCharacter;
            PlayerCanvas.gameObject.SetActive(true);

            HealthBar.Initialize(playerCharacter.Health);
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

        public void OnHealthChange(Component component, object data)
        {
            if (component.gameObject == PlayerController.PlayerCharacter.gameObject)
                HealthBar.UpdateHealthBars();
        }
    }
}
