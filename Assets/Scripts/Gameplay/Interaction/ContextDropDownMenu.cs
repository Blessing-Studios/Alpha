using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.Gameplay.Interation
{

    public class ContextDropDownMenu : MonoBehaviour
    {
        private List<Button> buttons = new();
        [SerializeField] private Button interactableDropDownPrefab;
        public void AddInteractables(List<IInteractable> interactables, Interactor interactor)
        {
            if (gameObject.activeSelf == true)
            {
                foreach (Button button in buttons)
                {
                    Destroy(button.gameObject);
                }
                buttons.Clear();

                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            foreach (IInteractable interactable in interactables)
            {
                Button interactableButton = Instantiate(interactableDropDownPrefab, this.transform);

                interactableButton.GetComponentInChildren<TextMeshProUGUI>().text = interactable.name;

                interactableButton.onClick.AddListener(() =>
                {
                    interactable.Interact(interactor);

                    foreach (Button button in buttons)
                    {
                        Destroy(button.gameObject);
                    }
                    buttons.Clear();

                    gameObject.SetActive(false);
                });

                buttons.Add(interactableButton);
            }
        }
    }
}
