using System;
using System.Threading.Tasks;
using Blessing.Core;
using Blessing.Core.ObjectPooling;
using Blessing.GameData;
using Blessing.Gameplay.TradeAndInventory;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Vivox;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Blessing.Services
{
    class ServicesHelper : MonoBehaviour
    {
        public static ServicesHelper Singleton { get; private set; }
        [SerializeField]
        bool m_InitiateVivoxOnAuthentication;

        static bool s_InitialLoad;

        Task m_SessionTask;

        ISession m_CurrentSession;
        public bool IsHost
        {
            get
            {
                if (m_CurrentSession != null)
                    return m_CurrentSession.IsHost;
                else
                    return false;
            }
        }
        public ISession CurrentSession { get { return m_CurrentSession; } }
        bool m_IsLeavingSession;

        private bool isDistributedAuthority { get { return NetworkManager.Singleton.NetworkConfig.NetworkTopology == NetworkTopologyTypes.DistributedAuthority; } }
        void Awake()
        {
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Singleton = this;
            }
        }

        async void Start()
        {
            await UnityServices.InitializeAsync();

            // if (!s_InitialLoad)
            // {
            //     s_InitialLoad = true;
            //     GameplayEventHandler.LoadMainMenuScene();
            // }

            NetworkManager.Singleton.OnClientStopped += OnClientStopped;

            GameplayEventHandler.OnStartButtonPressed += OnStartButtonPressed;
            GameplayEventHandler.OnMapTravelTriggered += OnMapTravelTriggered;
            GameplayEventHandler.OnSinglePlayerButtonPressed += OnSinglePlayerButtonPressed;
            GameplayEventHandler.OnReturnToMainMenuButtonPressed += OnReturnToMainMenuButtonPressed;
            GameplayEventHandler.OnQuitGameButtonPressed += OnQuitGameButtonPressed;
            GameplayEventHandler.OnExitedSession += OnExitedSession;
        }

        void OnClientStopped(bool obj)
        {
            LeaveSession();
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }

            GameplayEventHandler.OnStartButtonPressed -= OnStartButtonPressed;
            GameplayEventHandler.OnMapTravelTriggered -= OnMapTravelTriggered;
            GameplayEventHandler.OnSinglePlayerButtonPressed -= OnSinglePlayerButtonPressed;
            GameplayEventHandler.OnReturnToMainMenuButtonPressed -= OnReturnToMainMenuButtonPressed;
            GameplayEventHandler.OnQuitGameButtonPressed -= OnQuitGameButtonPressed;
            GameplayEventHandler.OnExitedSession -= OnExitedSession;
        }

        async void OnStartButtonPressed(string playerName, string sessionName)
        {
            var connectTask = ConnectToSession(playerName, sessionName);
            await connectTask;
            GameplayEventHandler.ConnectToSessionComplete(connectTask);
        }

        async void OnMapTravelTriggered(string playerName, string sessionName, SceneReference scene)
        {
            // TODO: Refatorar método
            await Awaitable.WaitForSecondsAsync(2);

            if (m_CurrentSession != null && isDistributedAuthority)
            {
                try
                {
                    await m_CurrentSession.LeaveAsync();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
            else
            {
                NetworkManager.Singleton.Shutdown();

                while (NetworkManager.Singleton.IsHost == true)
                {
                    await Awaitable.WaitForSecondsAsync(0.1f);
                }
            }

            ExitedSession();

            Debug.Log(gameObject.name + ": UnloadSceneAsync - " + SceneManager.Singleton.CurrentScene.SceneName);
            await UnitySceneManager.UnloadSceneAsync(SceneManager.Singleton.CurrentScene.SceneName);

            // TODO: adicionar cena de transição            

            // Connect to new session
            if (isDistributedAuthority)
            {
                Task connectTask = ConnectToSession(playerName, sessionName);
                await connectTask;
            }
            else
            {
                ConnectToSinglePlayerSession();
            }

            // If Host, load new Scene
            if (GameDataManager.Singleton.IsHost)
            {
                await UnitySceneManager.LoadSceneAsync(scene.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }

            // GameplayEventHandler.ConnectToNewMapComplete(connectTask);
        }

        async void OnSinglePlayerButtonPressed(SceneReference scene)
        {
            Debug.Log(gameObject.name + ": UnloadSceneAsync - " + SceneManager.Singleton.CurrentScene.SceneName);
            await UnitySceneManager.UnloadSceneAsync(SceneManager.Singleton.CurrentScene.SceneName);

            NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;

            ConnectToSinglePlayerSession();

            // If Host, load new Scene
            if (GameDataManager.Singleton.IsHost)
            {
                await UnitySceneManager.LoadSceneAsync(scene.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }

        private void ConnectToSinglePlayerSession()
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;
            NetworkManager.Singleton.StartHost();
            GameDataManager.Singleton.IsHost = true;
        }

        void OnReturnToMainMenuButtonPressed()
        {
            LeaveSession();

            NetworkManager.Singleton.Shutdown();

            GameManager.Singleton.ClearGameStates();

            SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
            SceneManager.Singleton.LoadAsync(GameManager.Singleton.MainMenuScene);

            // Close All game UI
            UIController.Singleton.CloseAll();
            UIController.Singleton.PlayerHUD.CloseHUD();
            UIController.Singleton.CloseDeathMenuUI();
        }

        void OnQuitGameButtonPressed()
        {
            Debug.Log("Quit Game");
            LeaveSession();
            Application.Quit();
        }
        void OnExitedSession()
        {

        }

        async void LeaveSession()
        {
            if (m_CurrentSession != null && !m_IsLeavingSession && isDistributedAuthority)
            {
                try
                {
                    m_IsLeavingSession = true;
                    await m_CurrentSession.LeaveAsync();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
                finally
                {
                    m_IsLeavingSession = false;
                    ExitedSession();
                }
            }
        }

        void SignInFailed(RequestFailedException obj)
        {
            Debug.LogWarning($"{nameof(SignedIn)} obj.ErrorCode {obj.ErrorCode}");
        }

        void SignedIn()
        {
            if (m_InitiateVivoxOnAuthentication)
            {
                LogInToVivox();
            }
        }

        async void LogInToVivox()
        {
            await VivoxService.Instance.InitializeAsync();

            var options = new LoginOptions
            {
                DisplayName = AuthenticationService.Instance.Profile,
                EnableTTS = true
            };
            VivoxService.Instance.LoggedIn += LoggedInToVivox;
            await VivoxService.Instance.LoginAsync(options);
        }

        static string GetRandomString(int length)
        {
            var r = new System.Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (char)r.Next('a', 'z' + 1);
            }

            return new string(result);
        }

        void LoggedInToVivox()
        {
            Debug.Log(nameof(LoggedInToVivox));
        }

        async Task SignIn()
        {
            try
            {
                AuthenticationService.Instance.SignInFailed += SignInFailed;
                AuthenticationService.Instance.SignedIn += SignedIn;
                AuthenticationService.Instance.SwitchProfile(GetRandomString(5));
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        async Task ConnectToSession(string playerName, string sessionName)
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.DistributedAuthority;

            if (AuthenticationService.Instance == null)
            {
                return;
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await SignIn();
            }

            // await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            GameDataManager.Singleton.PlayerName = playerName;

            if (string.IsNullOrEmpty(sessionName))
            {
                Debug.LogError("Session name is empty. Cannot connect.");
                return;
            }

            await ConnectThroughLiveService(sessionName);
        }

        async Task ConnectThroughLiveService(string sessionName)
        {
            try
            {
                var options = new SessionOptions()
                {
                    Name = sessionName,
                    MaxPlayers = 64,
                }.WithDistributedAuthorityNetwork();

                // TODO: checar erro: An item with the same key has already been added
                // TODO: checar erro: lobby not found
                // TODO: https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual/matchmake-session
                m_CurrentSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
                m_CurrentSession.RemovedFromSession += RemovedFromSession;
                m_CurrentSession.StateChanged += CurrentSessionOnStateChanged;

                Debug.Log("IsHost: " + m_CurrentSession.IsHost);
                GameDataManager.Singleton.IsHost = m_CurrentSession.IsHost;


                Debug.Log("MaxPlayers: " + m_CurrentSession.MaxPlayers);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        void RemovedFromSession()
        {
            ExitedSession();
        }

        void CurrentSessionOnStateChanged(SessionState sessionState)
        {
            if (sessionState != SessionState.Connected)
            {
                ExitedSession();
            }
        }

        void ExitedSession()
        {
            GameManager.Singleton.ClearGameStates();

            if (m_CurrentSession != null)
            {
                m_CurrentSession.RemovedFromSession -= RemovedFromSession;
                m_CurrentSession.StateChanged -= CurrentSessionOnStateChanged;
                m_CurrentSession = null;
                GameplayEventHandler.ExitedSession();
            }
        }
    }
}
