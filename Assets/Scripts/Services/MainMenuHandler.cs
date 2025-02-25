using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using System.Threading.Tasks;
using UnityEngine;

namespace Blessing.Services
{
    class MainMenuHandler : MonoBehaviour
    {
        // [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference SinglePlayerScene;
        // private SceneReference singlePlayerScene { get { return SinglePlayerScene.value as SceneReference; } }
        // public SceneReference FirstSinglePlayerScene { get { return singlePlayerScene;}}
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference MultiplayerScene;
        private SceneReference multiplayerScene { get { return MultiplayerScene.value as SceneReference; } }
        
        void Start()
        {
            GameplayEventHandler.OnConnectToSessionCompleted += OnConnectToSessionCompleted;
        }

        void OnDestroy()
        {
            GameplayEventHandler.OnConnectToSessionCompleted -= OnConnectToSessionCompleted;
        }

        public void OnConnectToSessionCompleted(Task task)
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("ConnectToSessionCompleted");
                if (GameDataManager.Singleton.IsHost)
                {
                    // Quando host carregar a cena, todos os clientes carregarão junto.
                    SceneManager.Singleton.LoadAsync(multiplayerScene); 
                }
                
                // Descarregar a cena presente, que deve ser o menu.
                SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
            }
        }
    }
}
