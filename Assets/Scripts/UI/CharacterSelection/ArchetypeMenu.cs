using System.Collections.Generic;
using Blessing.Gameplay.Characters;
using Blessing.Gameplay.Characters.Traits;
using Blessing.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.CharacterSelection
{
    public class ArchetypeMenu : MonoBehaviour
    {
        [SerializeField] private ArchetypeElement archetypeElementPrefab;
        private List<Button> buttons = new();
        void Start()
        {
            foreach (Archetype archetype in GameManager.Singleton.Archetypes)
            {
                ArchetypeElement archetypeElement = Instantiate(archetypeElementPrefab, this.transform);

                archetypeElement.Name.text = archetype.Label;
                archetypeElement.Icon.sprite = archetype.Icon;

                archetypeElement.Button.onClick.AddListener(() =>
                {
                    Debug.Log("archetypeButton.onClick");

                    foreach (Button button in buttons)
                    {
                        button.interactable = true;
                    }

                    buttons.Add(archetypeElement.Button);

                    archetypeElement.Button.interactable = false;

                    GameplayEventHandler.ArchetypeButtonPressed(archetype);
                });
            }
        }
    }
}
