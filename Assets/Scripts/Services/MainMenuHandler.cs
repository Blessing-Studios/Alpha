using Blessing.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using System.Threading.Tasks;
using UnityEngine;

namespace Blessing.Services
{
    class MainMenuHandler : MonoBehaviour
    {
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference PrototypeScene;
        private SceneReference prototypeScene { get { return PrototypeScene.value as SceneReference; } }
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
                    SceneManager.Singleton.LoadAsync(prototypeScene); 
                }
                
                // Descarregar a cena presente, que deve ser o menu.
                SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
            }
        }
    }
}
