using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("GameManager");
                    _instance = singletonObj.AddComponent<GameManager>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    public LobbyManager LobbyManager { get; private set; }

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localUser;
    public LocalPlayer LocalUser => _localUser;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    public event Action JoinLobbyEvent;

    private async void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
        LobbyManager = new LobbyManager();

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
            var lobby = await LobbyManager.CreateLobbyAsync(
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
            var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode, _localUser);

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

    public void StartNetworkGame()
    {
        SceneManager.LoadScene("OnlineGameScene");
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
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            _logger.Log($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    async Task BindLobby()
    {
        await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);
        //_localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
        //SetLobbyView();
        MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
    }

    public void LeaveLobby()
    {
        _localUser.ResetState();
        LobbyManager.LeaveLobbyAsync();
        ResetLocalLobby();
        LobbyList.Clear();
    }

    public void KickPlayer(string playerId)
    {
        LobbyManager.KickPlayer(playerId);
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
        await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
    }

    private void ResetLocalLobby()
    {
        _localLobby.ResetLobby();
    }

    public async void QueryLobbies()
    {
        LobbyList.QueryState.Value = LobbyQueryState.Fetching;
        var qr = await LobbyManager.GetLobbyListAsync();
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

    #region Teardown

    /// <summary>
    /// In builds, if we are in a lobby and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
    /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
    /// </summary>
    IEnumerator LeaveBeforeQuit()
    {
        ForceLeaveAttempt();
        yield return null;
        Application.Quit();
    }

    bool OnWantToQuit()
    {
        bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID.Value);
        StartCoroutine(LeaveBeforeQuit());
        return canQuit;
    }

    void OnDestroy()
    {
        ForceLeaveAttempt();
        LobbyManager.Dispose();
    }

    void ForceLeaveAttempt()
    {
        if (!string.IsNullOrEmpty(_localLobby?.LobbyID.Value))
        {
#pragma warning disable 4014
            LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
            _localLobby = null;
        }
    }

    #endregion
}
