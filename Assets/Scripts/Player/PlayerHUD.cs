using System;
using System.Collections.Generic;
using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.SkillsAndMagic;
using Blessing.HealthAndDamage;
using Blessing.UI;
using Blessing.UI.PlayerHUD;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.LookDev;
using UnityEngine.UI;

namespace Blessing.Player
{
    public class PlayerHUD : MonoBehaviour
    {
        public SegmentedHealthBar HealthBar;
        public BarUI WhiteManaBar;
        public ManaUI WhiteManaUI;
        public BarUI RedManaBar;
        public ManaUI RedManaUI;
        public BarUI GreenManaBar;
        public ManaUI GreenManaUI;
        public BarUI BlueManaBar;
        public ManaUI BlueManaUI;
        public BarUI BlackManaBar;
        public ManaUI BlackManaUI;
        public TextMeshProUGUI HpText;
        public TextMeshProUGUI GoldText;
        public Canvas PlayerCanvas;
        [field: SerializeField] public PlayerController PlayerController { get; private set; }
        [field: SerializeField] public PlayerCharacter PlayerCharacter { get; private set; }
        public List<AbilityUI> Abilities;

        [Header("Quick Use")]
        public QuickUseSlot SelectedQuickSlot;
        public List<QuickUseSlot> QuickUseSlots;

        [Header("Debug")]
        public GameObject DebugInfo;
        private List<TextMeshProUGUI> infoTexts = new();
        private Dictionary<ManaColor, BarUI> manaBarByColor = new();
        private Dictionary<ManaColor, ManaUI> manaUIByColor = new();
        private PlayerCharacterMana playerMana;
        void Awake()
        {
            PlayerController = GetComponent<PlayerController>();

            DebugInfo.SetActive(false);

            manaBarByColor.Add(ManaColor.White, WhiteManaBar);
            manaBarByColor.Add(ManaColor.Red, RedManaBar);
            manaBarByColor.Add(ManaColor.Green, GreenManaBar);
            manaBarByColor.Add(ManaColor.Blue, BlueManaBar);
            manaBarByColor.Add(ManaColor.Black, BlackManaBar);

            manaUIByColor.Add(ManaColor.White, WhiteManaUI);
            manaUIByColor.Add(ManaColor.Red, RedManaUI);
            manaUIByColor.Add(ManaColor.Green, GreenManaUI);
            manaUIByColor.Add(ManaColor.Blue, BlueManaUI);
            manaUIByColor.Add(ManaColor.Black, BlackManaUI);
        }
        void Start()
        {
            if (PlayerCanvas == null)
            {
                Debug.LogError(gameObject.name + " PlayerCanvas is missing");
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
        public void Initialize(PlayerCharacter playerCharacter, PlayerController playerController)
        {
            if (playerCharacter == null) Debug.LogError(gameObject.name + ": PlayerCharacter can't be null");

            PlayerCharacter = playerCharacter;
            PlayerController = playerController;

            playerMana = playerCharacter.Mana as PlayerCharacterMana;

            PlayerCanvas.gameObject.SetActive(true);
            HealthBar.Initialize(playerCharacter.Health);

            OpenHUD();
        }

        public void InitializeAbilitiesUI(List<CharacterAbility> characterAbilities)
        {
            for (int i = 0; i < characterAbilities.Count && i < Abilities.Count; i++)
            {
                Abilities[i].Initialize(characterAbilities[i]);
            }

            // Disable AbilitiesUI not being used
            for (int i = Abilities.Count - 1; i >= characterAbilities.Count; i--)
            {
                Abilities[i].gameObject.SetActive(false);
            }
        }

        public void InitializeQuickUseSlots(PlayerCharacter playerCharacter)
        {
            for (int i = 0; i < QuickUseSlots.Count && i < playerCharacter.Gear.UtilityInventories.Count; i++)
            {
                if (playerCharacter.Gear.UtilityInventories[i] != null)
                    QuickUseSlots[i].Initialize(playerCharacter, playerCharacter.Gear.UtilityInventories[i]);
            }
        }

        public void SelectQuickSlot(QuickUseSlot selectedQuickSlot)
        {
            foreach (QuickUseSlot quickUseSlot in QuickUseSlots)
            {
                quickUseSlot.UnSelectQuickUseSlot();
            }

            SelectedQuickSlot = selectedQuickSlot;

            SelectedQuickSlot.SelectQuickUseSlot();
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
        private void UpdateMana(ManaColor manaColor)
        {
            if (PlayerCharacter == null) return;

            if (manaBarByColor.ContainsKey(manaColor))
            {
                BarUI manaBar = manaBarByColor[manaColor];

                float maxValue = playerMana.GetMaxMana(manaColor);

                manaBar.Slider.maxValue = playerMana.GetMaxMana(manaColor);

                RectTransform backGroundRT = manaBar.BackGround.GetComponent<RectTransform>();
                RectTransform sliderRT = manaBar.Slider.GetComponent<RectTransform>();

                backGroundRT.sizeDelta = new Vector2(4 * maxValue, backGroundRT.sizeDelta.y);
                sliderRT.sizeDelta = new Vector2(4 * maxValue + 1, sliderRT.sizeDelta.y);
                manaBar.Slider.maxValue = maxValue;

                manaBar.Slider.value = playerMana.GetManaValue(manaColor);
            }

            if (manaUIByColor.ContainsKey(manaColor))
            {
                manaUIByColor[manaColor].UpdateValue(playerMana.GetManaValue(manaColor));
            }
        }

        public void OpenHUD()
        {
            gameObject.SetActive(true);
        }
        public void CloseHUD()
        {
            gameObject.SetActive(false);
        }

        public void OnHealthChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                HealthBar.UpdateHealthBars();
        }

        public void OnWhiteManaChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                UpdateMana(ManaColor.White);
        }
        public void OnRedManaChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                UpdateMana(ManaColor.Red);
        }
        public void OnGreenManaChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                UpdateMana(ManaColor.Green);
        }
        public void OnBlueManaChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                UpdateMana(ManaColor.Blue);
        }
        public void OnBlackManaChange(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                UpdateMana(ManaColor.Black);
        }
        public void OnPlayerAddEquipment(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                InitializeQuickUseSlots(PlayerCharacter);
        }
        public void OnPlayerRemoveEquipment(Component component, object data)
        {
            if (PlayerCharacter != null && component.gameObject == PlayerCharacter.gameObject)
                InitializeQuickUseSlots(PlayerCharacter);
        }
        public void OnAddUtilityItem(Component component, object data)
        {
            foreach (QuickUseSlot quickUseSlot in QuickUseSlots)
            {
                quickUseSlot.UpdateQuickUseSlot();
            }
        }
        public void OnRemoveUtilityItem(Component component, object data)
        {
            foreach (QuickUseSlot quickUseSlot in QuickUseSlots)
            {
                quickUseSlot.UpdateQuickUseSlot();
            }
        }
    }
}
