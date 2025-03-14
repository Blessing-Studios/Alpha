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
            }
        }

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
