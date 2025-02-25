using System;
using System.Threading.Tasks;
using Blessing.Core;
using Blessing.GameData;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Vivox;
using UnityEngine;

namespace Blessing.Services
{
    class ServicesHelper : MonoBehaviour
    {
        [SerializeField]
        bool m_InitiateVivoxOnAuthentication;

        static bool s_InitialLoad;

        Task m_SessionTask;

        ISession m_CurrentSession;
        bool m_IsLeavingSession;

        void Awake()
        {
            DontDestroyOnLoad(this);
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
            GameplayEventHandler.OnSinglePlayerButtonPressed += OnSinglePlayerButtonPressed;
            GameplayEventHandler.OnReturnToMainMenuButtonPressed += OnReturnToMainMenuButtonPressed;
            GameplayEventHandler.OnQuitGameButtonPressed += OnQuitGameButtonPressed;
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
            GameplayEventHandler.OnSinglePlayerButtonPressed -= OnSinglePlayerButtonPressed;
            GameplayEventHandler.OnReturnToMainMenuButtonPressed -= OnReturnToMainMenuButtonPressed;
            GameplayEventHandler.OnQuitGameButtonPressed -= OnQuitGameButtonPressed;
        }

        async void OnStartButtonPressed(string playerName, string sessionName)
        {
            var connectTask = ConnectToSession(playerName, sessionName);
            await connectTask;
            GameplayEventHandler.ConnectToSessionComplete(connectTask);
        }

        async void OnSinglePlayerButtonPressed(SceneReference scene)
        {   
            
            // Quando host carregar a cena, todos os clientes carregarão junto.
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene.SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

            NetworkManager.Singleton.NetworkConfig.NetworkTopology = NetworkTopologyTypes.ClientServer;   
            // Descarregar a cena presente, que deve ser o menu.
            SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
            ConnectToSinglePlayerSession();
        }

        private void ConnectToSinglePlayerSession()
        {
            NetworkManager.Singleton.StartHost();
            GameDataManager.Singleton.IsHost = true;
        }

        void OnReturnToMainMenuButtonPressed()
        {
            LeaveSession();
        }

        void OnQuitGameButtonPressed()
        {
            LeaveSession();
            Application.Quit();
        }

        async void LeaveSession()
        {
            if (m_CurrentSession != null && !m_IsLeavingSession)
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

            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);

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
