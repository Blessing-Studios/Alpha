using System;
using System.Collections.Generic;
using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Blessing.HealthAndDamage;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

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
        [Header("Debug")]
        public GameObject DebugInfo;
        private List<TextMeshProUGUI> infoTexts = new();
        void Awake()
        {
            PlayerController = GetComponent<PlayerController>();

            DebugInfo.SetActive(false);

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

                return;
            }
        }

        private void AddInfoText(string[] infos)
        {
            int sizeDiff = infos.Length - infoTexts.Count;

            if (sizeDiff > 0)
            {
                for (int i = 0; i < sizeDiff; i++)
                {
                    GameObject newGameObject = new("InfoText");

                    newGameObject.AddComponent<RectTransform>().sizeDelta = new Vector2(500, 70);

                    TextMeshProUGUI textComp = newGameObject.AddComponent<TextMeshProUGUI>();
                    textComp.fontSize = 30;
                    newGameObject.transform.SetParent(DebugInfo.transform, false);

                    infoTexts.Add(textComp);
                }
            }

            for (int i = 0; i < infoTexts.Count; i++)
            {
                infoTexts[i].text = infos[i];
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
            HandleDebugInfo();
        }

        private void HandleDebugInfo()
        {
            if (PlayerCharacter == null) return;

            if (PlayerController.ShowDebug == false)
            {
                DebugInfo.SetActive(false);
                return;
            }

            if (PlayerController.ShowDebug)
            {
                DebugInfo.SetActive(true);
            }

            string triggerActionName;
            if (PlayerCharacter.TriggerAction != null)
            {
                triggerActionName = PlayerCharacter.TriggerAction.Name;
            }
            else
            {
                triggerActionName = "None";
            }

            string[] infos = new string[]{
                "AiCharacter Spawned: " + GameManager.Singleton.AiCharacterSpawned,
                "Player Character Found: " + (PlayerController.PlayerCharacter != null),
                "PlayerName: " + PlayerCharacter.GetPlayerOwnerName(),
                "HasAuthority: " + PlayerCharacter.HasAuthority,
                "Validate Owner: " + GameDataManager.Singleton.ValidateOwner(PlayerCharacter.GetPlayerOwnerName()),
                "CanMove: " + PlayerCharacter.MovementController.CanMove,
                "CanGiveInputs: " + PlayerCharacter.CanGiveInputs,
                "Character IsInitialized: " + PlayerCharacter.IsInitialized + " - " + PlayerController.WasInitialize,
                "Movement Input: " + PlayerCharacter.MovementController.GetCurrentMovementInput(),
                "Trigger Action: " + triggerActionName,
                "Last Rnd Number: " + PlayerCharacter.TestRandomFloat
            };

            AddInfoText(infos);
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
            if (PlayerController.PlayerCharacter != null && component.gameObject == PlayerController.PlayerCharacter.gameObject)
                HealthBar.UpdateHealthBars();
        }
    }
}
