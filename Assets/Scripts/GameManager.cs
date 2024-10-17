using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
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

        var result = await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName.Substring(0, 30));

        _logger.Log($"Services Authentification Result: {result}");
    }

    void AuthenticatePlayer()
    {
        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var randomName = $"Player{UnityEngine.Random.Range(1, 10)}";

        _localUser.ID.Value = localId;
        _localUser.DisplayName.Value = randomName;
    }

    public async void CreateLobby(string name, bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            var lobby = await LobbyManager.Instance.CreateLobbyAsync(
                name,
                maxPlayers,
                isPrivate);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);
            await CreateLobby();

            MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            // SetGameState(GameState.JoinMenu);
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            _logger.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    async Task CreateLobby()
    {
        _localUser.IsHost.Value = true;
        //_localLobby.onUserReadyChange = OnPlayersReady;
        try
        {
            // TODO: BINDING LOCAL LOBBY TO REMOTE LOBBY
            await BindLobby();
        }
        catch (LobbyServiceException exception)
        {
            // SetGameState(GameState.JoinMenu);
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            _logger.Log($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    async Task BindLobby()
    {
        // TODO: BINDING LOCAL LOBBY TO REMOTE LOBBY
        //await LobbyManager.Instance.BindLocalLobbyToRemote(m_LocalLobby.LobbyID.Value, m_LocalLobby);
        //m_LocalLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
        //SetLobbyView();
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
        //await LobbyManager.Instance.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(m_LocalUser));
    }
}
