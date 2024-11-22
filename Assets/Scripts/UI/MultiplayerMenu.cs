using Blessing.Services;
using Blessing.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MultiplayerMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_InputField playerNameField;
        [SerializeField] private TMP_InputField sessionNameField;
        void Awake() 
        {

            startButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("startButton");
                string playerName = playerNameField.text;
                string sessionName = sessionNameField.text;

                // TODO: validar playerName e sessionName
                startButton.interactable = false;
                GameDataManager.Singleton.PlayerName = playerName;
                GameplayEventHandler.StartButtonPressed(playerName, sessionName);
            });
        }
    }
}
