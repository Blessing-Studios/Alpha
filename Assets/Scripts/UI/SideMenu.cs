using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SideMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button prototypeButton;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference PrototypeScene;
        private SceneReference prototypeScene { get { return PrototypeScene.value as SceneReference; } }
    
        [SerializeField] private Button multiplayerButton;
        [SerializeField] GameObject multiplayerPanel;
        [SerializeField] private Button optionsButton;
        [SerializeField] private GameObject firstSelectedGameObject;
        
        void Awake() 
        {
            multiplayerPanel.SetActive(false);

            prototypeButton.onClick.AddListener(() => {
                if (ShowDebug) Debug.Log("prototypeButton");
                SceneManager.Singleton.LoadAsync(prototypeScene); 
                SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
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
