using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class SideMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
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

            singlePlayerButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("prototypeButton");

                GameDataManager.Singleton.PlayerName = "Player1";

                GameplayEventHandler.SinglePlayerButtonPressed(singlePlayerScene);
            });

            multiplayerButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("multiplayerButton");
                multiplayerPanel.SetActive(true);
                
            });

            optionsButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("SettingsButton");
                Debug.Log("multiplayerButton: not done");
                
            });
        }

        void Start()
        {
            if (firstSelectedGameObject == null)
            {
                Debug.LogError(gameObject.name + ": firstSelectedGameObject is missing");
            }
            
            // Call GameManager to set Selected GameObject
            GameManager.Singleton.SetSelectedGameObject(firstSelectedGameObject);
        }
    }
}
