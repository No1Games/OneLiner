using System;
using System.Collections.Generic;
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
    public LocalPlayer LocalUser => _localUser;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    public event Action JoinLobbyEvent;

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

        var randomName = RandomNameGenerator.GetName();

        _localUser.ID.Value = localId;

        SetLocalUserName(randomName);
    }

    public async void CreateLobby(string name, bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            var lobby = await LobbyManager.Instance.CreateLobbyAsync(
                name,
                maxPlayers,
                isPrivate,
                _localUser);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);
            await CreateLobby();

            MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
            JoinLobbyEvent?.Invoke();
        }
        catch (LobbyServiceException exception)
        {
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            _logger.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async void JoinLobby(string lobbyID, string lobbyCode)
    {
        try
        {
            var lobby = await LobbyManager.Instance.JoinLobbyAsync(lobbyID, lobbyCode, _localUser);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);
            await JoinLobby();
        }
        catch (LobbyServiceException exception)
        {
            // SetGameState(GameState.JoinMenu);
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            _logger.Log($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task JoinLobby()
    {
        _localUser.IsHost.ForceSet(false);
        await BindLobby();
    }

    async Task CreateLobby()
    {
        _localUser.IsHost.Value = true;
        //_localLobby.onUserReadyChange = OnPlayersReady;
        try
        {
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
        await LobbyManager.Instance.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
        //_localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
        //SetLobbyView();
        MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
    }

    public void LeaveLobby()
    {
        _localUser.ResetState();
        LobbyManager.Instance.LeaveLobbyAsync();
        ResetLocalLobby();
        //LobbyList.Clear();
    }

    public void KickPlayer(string playerId)
    {
        LobbyManager.Instance.KickPlayer(playerId);
    }

    public void SetLocalUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.Log("Empty Name not allowed."); // Lobby error type, then HTTP error type.
            return;
        }

        _logger.Log($"Change user name to: {name}");

        _localUser.DisplayName.Value = name;
        SendLocalUserData();
    }

    async void SendLocalUserData()
    {
        await LobbyManager.Instance.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
    }

    private void ResetLocalLobby()
    {
        _localLobby.ResetLobby();
    }

    public async void QueryLobbies()
    {
        LobbyList.QueryState.Value = LobbyQueryState.Fetching;
        var qr = await LobbyManager.Instance.GetLobbyListAsync();
        if (qr == null)
        {
            return;
        }

        SetCurrentLobbies(LobbyConverters.QueryToLocalList(qr));
    }

    void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
    {
        var newLobbyDict = new Dictionary<string, LocalLobby>();
        foreach (var lobby in lobbies)
            newLobbyDict.Add(lobby.LobbyID.Value, lobby);

        LobbyList.CurrentLobbies = newLobbyDict;
        LobbyList.QueryState.Value = LobbyQueryState.Fetched;
    }
}
