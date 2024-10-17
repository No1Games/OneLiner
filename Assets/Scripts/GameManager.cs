using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localUser;

    private async void Awake()
    {
        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };

        await InitializeServices();
        AuthenticatePlayer();
    }

    public async Task InitializeServices()
    {
        string serviceProfileName = $"player{Guid.NewGuid()}";

        await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName);
    }

    void AuthenticatePlayer()
    {
        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var randomName = $"Player{UnityEngine.Random.Range(1, 10)}";

        _localUser.ID.Value = localId;
        _localUser.DisplayName.Value = randomName;
    }

    public void SetLocalUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.Log("Empty Name not allowed."); // Lobby error type, then HTTP error type.
            return;
        }

        _localUser.DisplayName.Value = name;
        SendLocalUserData();
    }

    async void SendLocalUserData()
    {
        await LobbyManager.Instance.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(m_LocalUser));
    }
}
