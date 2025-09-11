using System;
using System.Collections.Generic;
using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.DataPersistence;
using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Guild;
using Blessing.Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


namespace Blessing.UI.CharacterSelection
{
    public class CharacterMenu : MonoBehaviour, IDataPersistence
    {
        public GameObject ArchetypeColumn;
        public CharacterListMenu CharacterListMenu;

        [Header("Character Info")]
        public GameObject CharacterInfoColumn;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI rankAndClassText;
        [SerializeField] private Button confirmCharacterButton;
        
        [Header("Create Character")]
        public GameObject CreateCharacterColumn;
        public GameObject ClassInfoColumn;
        [SerializeField] private TMP_InputField characterNameField;
        [SerializeField] private TextMeshProUGUI characterArchetypeText;
        [SerializeField] private TextMeshProUGUI ArchetypeDescriptionText;
        [SerializeField] private Button confirmCharacterCreateButton;
        private SceneReference mainMenuScene { get { return GameManager.Singleton.MainMenuScene; } }
        [SerializeField] private CharacterData characterSelected;
        public List<CharacterData> Characters = new();
        void Awake()
        {
            confirmCharacterCreateButton.onClick.AddListener(() =>
            {
                Debug.Log("confirmCharacterButton");

                if (ValidateCharacterInfo())
                {
                    Debug.Log(gameObject.name + " SaveData - " + characterSelected.Name);

                    GameDataManager.Singleton.SaveGame();
                    GameDataManager.Singleton.CharacterSelected = characterSelected;

                    SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
                    SceneManager.Singleton.LoadAsync(mainMenuScene);
                }
            });

            confirmCharacterButton.onClick.AddListener(() =>
            {
                Debug.Log("confirmCharacterButton");
                
                GameDataManager.Singleton.SaveGame();
                GameDataManager.Singleton.CharacterSelected = characterSelected;

                SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
                SceneManager.Singleton.LoadAsync(mainMenuScene);
            });
        }

        private bool ValidateCharacterInfo()
        {
            if (characterNameField.text == "") 
            {
                Debug.LogWarning("Character Name is Missing");
                return false;
            }

            characterSelected.Name = characterNameField.text;
            return true;
        }

        void Start()
        {
            if (ArchetypeColumn == null)
            {
                Debug.LogError(gameObject.name + ": ArchetypeColumn is missing");
            }

            if (confirmCharacterButton == null)
            {
                Debug.LogError(gameObject.name + ": confirmCharacterButton is missing");
            }

            if (characterNameField == null)
            {
                Debug.LogError(gameObject.name + ": characterNameField is missing");
            }

            GameDataManager.Singleton.UpdatePersistenceObjectsList();
            GameplayEventHandler.OnArchetypeButtonPressed += OnArchetypeButtonPressed;

            GameDataManager.Singleton.LoadGame();

            ArchetypeColumn.SetActive(false);
            CreateCharacterColumn.SetActive(false);
            ClassInfoColumn.SetActive(false);
            CharacterInfoColumn.SetActive(true);
            CharacterListMenu.gameObject.SetActive(true);

            CharacterListMenu.CreateCharacterSlots();

            characterNameText.text = "";
            rankAndClassText.text = "";
            ArchetypeDescriptionText.text = "CLASS INFO";
        }

        void OnDestroy()
        {
            GameplayEventHandler.OnArchetypeButtonPressed -= OnArchetypeButtonPressed;
        }

        private void OnArchetypeButtonPressed(Archetype archetype)
        {
            characterSelected.ArchetypeId = archetype.Id;
            characterArchetypeText.text = archetype.Label;
            
            // Update informações
            ArchetypeDescriptionText.text = "CLASS INFO <br> <br> " + archetype.Description;
        }

        public void SelectCharacter(CharacterData character)
        {
            gameObject.SetActive(true);
            ArchetypeColumn.SetActive(false);
            CreateCharacterColumn.SetActive(false);
            ClassInfoColumn.SetActive(false);
            CharacterInfoColumn.SetActive(true);
            CharacterListMenu.gameObject.SetActive(true);

            characterSelected = character;

            characterNameText.text = character.Name;

            Archetype archetype = GameManager.Singleton.GetArchetypeById(character.ArchetypeId);
            
            Rank rank = new Rank(character.RankScore, character.RankStrike);
            rankAndClassText.text = $"{rank.Label} Rank {archetype.Label}";
        }

        public void CreateNewCharacter()
        {
            gameObject.SetActive(true);
            ArchetypeColumn.SetActive(true);
            CreateCharacterColumn.SetActive(true);
            ClassInfoColumn.SetActive(true);
            CharacterInfoColumn.SetActive(false);
            CharacterListMenu.gameObject.SetActive(false);

            characterSelected = new CharacterData();

            Characters.Add(characterSelected);

            characterNameField.interactable =  true;
        }

        public void DeleteCharacter()
        {
            Characters.Remove(characterSelected);

            characterNameText.text = "";
            rankAndClassText.text = "";

            characterSelected = null;
        }

        public void LoadData<T>(T gameData) where T : Data
        {
            Debug.Log("Load Teste");
            PlayerData playerData = gameData as PlayerData;

            Characters = playerData.Characters;
        }

        public void SaveData<T>(ref T gameData) where T : Data
        {
            PlayerData playerData = gameData as PlayerData;

            playerData.Characters = Characters;
        }
    }
}
