using System;
using System.Threading.Tasks;
using Blessing.Core;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Blessing.Core.ScriptableObjectDropdown;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Blessing.Network
{
    public class ConnectionManager : MonoBehaviour
    {
        private string _profileName;
        private string _sessionName;
        private int _maxPlayers = 10;
        private ConnectionState _state = ConnectionState.Disconnected;
        private ISession _session;
        private NetworkManager m_NetworkManager;

        private enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected,
        }

        private async void Awake()
        {
            Debug.Log(gameObject.name + ": Awake ConnectionManager");
            m_NetworkManager = GetComponent<NetworkManager>();
            m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
            await UnityServices.InitializeAsync();
        }

        private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
        {
            if (m_NetworkManager.LocalClient.IsSessionOwner)
            {
                Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (m_NetworkManager.LocalClientId == clientId)
            {
                GameManager.Singleton.OnClientConnected(clientId);

                // Carregar cena
                // SceneManager.Singleton.LoadAsync(prototypeScene); 
                // SceneManager.Singleton.Unload(SceneManager.Singleton.CurrentScene);
            }
        }

        // private void OnGUI()
        // {
        //     if (_state == ConnectionState.Connected)
        //         return;

        //     GUI.enabled = _state != ConnectionState.Connecting;

        //     using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        //     {
        //         GUILayout.Label("Profile Name", GUILayout.Width(100));
        //         _profileName = GUILayout.TextField(_profileName);
        //     }

        //     using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        //     {
        //         GUILayout.Label("Session Name", GUILayout.Width(100));
        //         _sessionName = GUILayout.TextField(_sessionName);
        //     }

        //     GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(_profileName) && !string.IsNullOrEmpty(_sessionName);

        //     // if (GUILayout.Button("Create or Join Session"))
        //     // {
        //     //     CreateOrJoinSessionAsync();
        //     // }
        // }

        private void OnDestroy()
        {
            _session?.LeaveAsync();
        }

        // TODO: fazer essa função a responsável pela conexão

        private async Task CreateOrJoinSessionAsync()
        {
            _state = ConnectionState.Connecting;

            try
            {
                AuthenticationService.Instance.SwitchProfile(_profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var options = new SessionOptions()
                {
                    Name = _sessionName,
                    MaxPlayers = _maxPlayers
                }.WithDistributedAuthorityNetwork();

                _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

                _state = ConnectionState.Connected;
            }
            catch (Exception e)
            {
                _state = ConnectionState.Disconnected;
                Debug.LogException(e);
            }
        }
    }
}
