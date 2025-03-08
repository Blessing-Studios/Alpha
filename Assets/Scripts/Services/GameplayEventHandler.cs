using Blessing.Core;
using Unity.Netcode;
using System;
using System.Threading.Tasks;

namespace Blessing.Services
{
    // Não está sendo usado, estudar se deve ser deletado
    static class GameplayEventHandler
    {
        internal static event Action<NetworkObject> OnNetworkObjectDespawned;
        internal static event Action<NetworkObject, ulong, ulong> OnNetworkObjectOwnershipChanged;
        internal static event Action<string, string> OnStartButtonPressed;
        internal static event Action<string, string, SceneReference> OnMapTravelTriggered;
        internal static event Action<SceneReference> OnSinglePlayerButtonPressed;
        internal static event Action OnReturnToMainMenuButtonPressed;
        internal static event Action OnQuitGameButtonPressed;
        internal static event Action<Task> OnConnectToSessionCompleted;
        internal static event Action<Task> OnConnectToNewMapCompleted;
        internal static event Action OnExitedSession;

        internal static void NetworkObjectDespawned(NetworkObject networkObject)
        {
            OnNetworkObjectDespawned?.Invoke(networkObject);
        }

        internal static void NetworkObjectOwnershipChanged(NetworkObject networkObject, ulong previous, ulong current)
        {
            OnNetworkObjectOwnershipChanged?.Invoke(networkObject, previous, current);
        }

        internal static void StartButtonPressed(string playerName, string sessionName)
        {
            OnStartButtonPressed?.Invoke(playerName, sessionName);
        }

        internal static void MapTravelTriggered(string playerName, string sessionName, SceneReference scene)
        {
            OnMapTravelTriggered?.Invoke(playerName, sessionName, scene);
        }

        internal static void SinglePlayerButtonPressed(SceneReference scene)
        {
            OnSinglePlayerButtonPressed?.Invoke(scene);
        }

        internal static void ReturnToMainMenuPressed()
        {
            OnReturnToMainMenuButtonPressed?.Invoke();
        }

        internal static void QuitGamePressed()
        {
            OnQuitGameButtonPressed?.Invoke();
        }

        internal static void ConnectToSessionComplete(Task task)
        {
            OnConnectToSessionCompleted?.Invoke(task);
        }

        internal static void ConnectToNewMapComplete(Task task)
        {
            OnConnectToNewMapCompleted?.Invoke(task);
        }
        internal static void ExitedSession()
        {
            OnExitedSession?.Invoke();
        }
    }
}
