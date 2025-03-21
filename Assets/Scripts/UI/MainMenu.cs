using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class MainMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private TextMeshProUGUI characterNameDisplay;
        [SerializeField] private Button characterSelectionButton;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference CharacterSelectionScene;
        private SceneReference characterSelectionScene { get { return CharacterSelectionScene.value as SceneReference; } }
        [SerializeField] private Button singlePlayerButton;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference SinglePlayerScene;
        private SceneReference singlePlayerScene { get { return SinglePlayerScene.value as SceneReference; } }
    
        [SerializeField] private Button multiplayerButton;
        [SerializeField] GameObject multiplayerPanel;
        [SerializeField] private Button optionsButton;
        [SerializeField] private GameObject firstSelectedGameObject;
        
        void Awake() 
        {
            multiplayerPanel.SetActive(false);

            characterSelectionButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("characterSelectionButton");

                characterSelectionButton.interactable = false;

                SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);

                SceneManager.Singleton.LoadAsync(characterSelectionScene);
            });

            singlePlayerButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("SinglePlayerButton");

                GameDataManager.Singleton.PlayerName = "Player1";
                singlePlayerButton.interactable = false;
                GameplayEventHandler.SinglePlayerButtonPressed(singlePlayerScene);
            });

            multiplayerButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("MultiplayerButton");
                multiplayerPanel.SetActive(true);
                
            });

            optionsButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("SettingsButton");
                Debug.Log("multiplayerButton: not done");
                
            });

            if (GameDataManager.Singleton.CharacterSelected.Id != "")
            {
                characterNameDisplay.text = GameDataManager.Singleton.CharacterSelected.Name;
            }
            else
            {
                characterNameDisplay.text = "";
                singlePlayerButton.interactable = false;
                multiplayerButton.interactable = false;
            }
        }

        void Start()
        {
            // Fazer mensagens de erros para caso falte injetar dependências
            if (firstSelectedGameObject == null)
            {
                Debug.LogError(gameObject.name + ": firstSelectedGameObject is missing");
            }
            
            // Call GameManager to set Selected GameObject
            GameManager.Singleton.SetSelectedGameObject(firstSelectedGameObject);
        }   
    }
}
