using System.Collections.Generic;
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
    public class CharacterListMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button createNewButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private CharacterSlot characterSlotPrefab;
        private List<Button> buttons = new();
        private List<CharacterSlot> characterSlots = new();
        [SerializeField] private CharacterMenu characterMenu;
        [SerializeField] private GameObject characterList;
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
            });

            deleteButton.onClick.AddListener(() =>
            {
                Debug.Log("deleteButton");
                foreach(Button button in buttons)
                {
                    button.interactable = true;
                }

                deleteButton.interactable = false;
                
                characterMenu.DeleteCharacter();

                // It will recreate all buttons so the deleted character disappear fom list
                CreateCharacterSlots();
            });

            buttons.Add(deleteButton);
        }

        public void CreateCharacterSlots()
        {   
            List<CharacterData> characters = characterMenu.Characters;

            foreach(CharacterSlot slot in characterSlots)
            {
                slot.gameObject.SetActive(false);
            }

            characterSlots.Clear();

            if (characters.Count == 0) return;

            foreach(CharacterData character in characters)
            {
                CharacterSlot characterSlot = Instantiate(characterSlotPrefab, characterList.transform);

                Button characterButton = characterSlot.GetComponent<Button>();

                characterSlot.Name.text = character.Name;

                Rank rank = new Rank(character.RankScore, character.RankStrike);
                Archetype archetype = GameManager.Singleton.GetArchetypeById(character.ArchetypeId);

                characterSlot.RankAndClass.text = $"{rank.Label} Rank {archetype.Label}";
                characterSlot.Icon.sprite = archetype.Icon;
                
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
                characterSlots.Add(characterSlot);
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
                Debug.LogError(gameObject.name + ": characterMenu is missing");
            }

            if (createNewButton == null)
            {
                Debug.LogError(gameObject.name + ": createNewButton is missing");
            }

            if (deleteButton == null)
            {
                Debug.LogError(gameObject.name + ": deleteButton is missing");
            }

            // Call GameManager to set Selected GameObject
            GameManager.Singleton.SetSelectedGameObject(firstSelectedGameObject);
        }
    }
}
