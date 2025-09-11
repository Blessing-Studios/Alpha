using System;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        public Button ContinueButton;
        public Button MainMenuButton;
        public Button OptionsButton;
        public Button QuitButton;
        
        void Awake()
        {
            ContinueButton.onClick.AddListener(() =>
            {
                // Return go the Game

                UIController.Singleton.ClosePauseMenuUI();
            });

            MainMenuButton.onClick.AddListener(() =>
            {
                // Return to Main Menu

                GameplayEventHandler.ReturnToMainMenuPressed();
            });

            OptionsButton.onClick.AddListener(() =>
            {
                // Go to Options Menu
            });

            QuitButton.onClick.AddListener(() =>
            {
                GameplayEventHandler.QuitGamePressed();
            });
        }

        void Start()
        {
            if (ContinueButton == null)
            {
                Debug.LogError(gameObject.name + ": ContinueButton is missing");
            }

            if (MainMenuButton == null)
            {
                Debug.LogError(gameObject.name + ": MainMenuButton is missing");
            }

            if (OptionsButton == null)
            {
                Debug.LogError(gameObject.name + ": OptionsButton is missing");
            }

            if (QuitButton == null)
            {
                Debug.LogError(gameObject.name + ": QuitButton is missing");
            }
        }
        public void OpenPauseMenuUI()
        {
            gameObject.SetActive(true);

            // Atualizar informações do menu de pausa
        }
        public void ClosePauseMenuUI()
        {
            gameObject.SetActive(false);
        }
    }
}
