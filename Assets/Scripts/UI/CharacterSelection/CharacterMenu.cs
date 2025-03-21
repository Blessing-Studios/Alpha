using System;
using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.DataPersistence;
using Blessing.GameData;
using Blessing.Gameplay.Characters;
using Blessing.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Blessing.UI.CharacterSelection
{
    public class CharacterMenu : MonoBehaviour, IDataPersistence
    {
        public ArchetypeMenu ArchetypeMenu;
        [SerializeField] private Button confirmCharacterButton;
        
        [Header("Character Info")]
        [SerializeField] private TMP_InputField characterNameField;
        [SerializeField] private TextMeshProUGUI characterArchetypeText;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference MainMenuScene;
        private SceneReference mainMenuScene { get { return MainMenuScene.value as SceneReference; } }

        private CharacterData characterSelected;
        void Awake()
        {
            confirmCharacterButton.onClick.AddListener(() =>
            {
                Debug.Log("confirmCharacterButton");

                if (ValidateCharacterInfo())
                {
                    GameDataManager.Singleton.SaveGame();
                    GameDataManager.Singleton.CharacterSelected = characterSelected;

                    SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
                    SceneManager.Singleton.LoadAsync(mainMenuScene);
                }
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
            if (ArchetypeMenu == null)
            {
                Debug.LogError(gameObject.name + ": ArchetypeMenu is missing");
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
        }

        public void SelectCharacter(CharacterData character)
        {
            gameObject.SetActive(true);
            ArchetypeMenu.gameObject.SetActive(false);
            characterSelected = character;

            characterNameField.text = character.Name;

            Archetype archetype = GameManager.Singleton.GetArchetypeById(character.ArchetypeId);
            
            if (archetype != null)
            {
                characterArchetypeText.text = archetype.Label;
            }

            characterNameField.interactable =  false;
        }

        public void CreateNewCharacter()
        {
            gameObject.SetActive(true);
            ArchetypeMenu.gameObject.SetActive(true);
            characterSelected = new CharacterData();

            characterNameField.interactable =  true;
        }

        public void LoadData<T>(T gameData) where T : Data
        {
            //
        }

        public void SaveData<T>(ref T gameData) where T : Data
        {
            PlayerData playerData = gameData as PlayerData;

            foreach(CharacterData character in playerData.Characters)
            {
                if (character.Id == characterSelected.Id)
                {
                    return;
                }
            }
            Debug.Log(gameObject.name + " SaveData - " + characterSelected.Name);
            playerData.Characters.Add(characterSelected);
        }
    }
}
