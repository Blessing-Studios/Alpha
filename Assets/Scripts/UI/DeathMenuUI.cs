using System;
using Blessing.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class DeathMenuUI : MonoBehaviour
    {
        public Button ReviveButton;
        public Button MainMenuButton;

        void Awake()
        {
            ReviveButton.onClick.AddListener(() =>
            {
                // Revive Death Character
            });

            MainMenuButton.onClick.AddListener(() =>
            {
                // Return go the Game

                GameplayEventHandler.ReturnToMainMenuPressed();
            });
        }

        void Start()
        {
            if (ReviveButton == null)
            {
                Debug.LogError(gameObject.name + ": ReviveButton is missing");
            }

            if (MainMenuButton == null)
            {
                Debug.LogError(gameObject.name + ": MainMenuButton is missing");
            }
        }

        public void OpenDeathMenuUI()
        {
            gameObject.SetActive(true);
        }

        public void CloseDeathMenuUI()
        {
            gameObject.SetActive(false);
        }
    }
}
