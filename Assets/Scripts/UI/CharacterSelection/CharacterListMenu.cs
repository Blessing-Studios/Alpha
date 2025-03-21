using System.Collections.Generic;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.DataPersistence;
using Blessing.GameData;
using Blessing.Services;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.CharacterSelection
{
    public class CharacterListMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button createNewButton;
        [SerializeField] private Button characterSlotPrefab;
        private List<Button> buttons = new();
        [SerializeField] private CharacterMenu characterMenu;
        [SerializeField] private GameObject firstSelectedGameObject;
        void Awake()
        {
            createNewButton.onClick.AddListener(() =>
            {
                Debug.Log("createNewButton");
                foreach(Button button in buttons)
                {
                    button.interactable = true;
                }

                createNewButton.interactable = false;
                
                characterMenu.CreateNewCharacter();

                // Button newCharacterButton = Instantiate(characterSlotPrefab, this.transform);

                // newCharacterButton.onClick.AddListener(() =>
                // {
                //     Debug.Log("newCharacter.onClick");
                // });
            });

            buttons.Add(createNewButton);

            List<CharacterData> characters = GameDataManager.Singleton.GetCharacters();

            if (characters.Count == 0) return;

            foreach(CharacterData character in characters)
            {
                Button characterButton = Instantiate(characterSlotPrefab, this.transform);

                characterButton.GetComponentInChildren<TextMeshProUGUI>().text = character.Name;

                characterButton.onClick.AddListener(() =>
                {
                    foreach(Button button in buttons)
                    {
                        button.interactable = true;
                    }

                    characterButton.interactable = false;

                    characterMenu.SelectCharacter(character);
                });

                buttons.Add(characterButton);
            }
        }

        void Start()
        {
            if (firstSelectedGameObject == null)
            {
                Debug.LogError(gameObject.name + ": firstSelectedGameObject is missing");
            }

            if (characterMenu == null)
            {
                Debug.LogError(gameObject.name + ": createCharacterMenu is missing");
            }

            // Call GameManager to set Selected GameObject
            GameManager.Singleton.SetSelectedGameObject(firstSelectedGameObject);

            characterMenu.gameObject.SetActive(false);
        }
    }
}
