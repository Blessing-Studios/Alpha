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
        [SerializeField] private Button archetypeButtonPrefab;
        private List<Button> buttons = new();
        void Start()
        {
            foreach (Archetype archetype in GameManager.Singleton.Archetypes)
            {
                Button archetypeButton = Instantiate(archetypeButtonPrefab, this.transform);

                archetypeButton.GetComponentInChildren<TextMeshProUGUI>().text = archetype.Label;

                archetypeButton.onClick.AddListener(() =>
                {
                    Debug.Log("archetypeButton.onClick");

                    foreach (Button button in buttons)
                    {
                        button.interactable = true;
                    }

                    buttons.Add(archetypeButton);

                    archetypeButton.interactable = false;

                    GameplayEventHandler.ArchetypeButtonPressed(archetype);
                });
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
