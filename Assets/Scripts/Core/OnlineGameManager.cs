using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineGameManager : MonoBehaviour
{
    static OnlineGameManager _instance;
    public static OnlineGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OnlineGameManager>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("OnlineGameManager");
                    _instance = singletonObj.AddComponent<OnlineGameManager>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    #region Lobby Fields and Props

    public LobbyManager LobbyManager { get; private set; }

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localUser;
    private bool _doesNeedCleanup;

    public LocalPlayer LocalUser => _localUser;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    #endregion

    public event Action AllPlayersReadyEvent;
    public event Action PlayerNotReadyEvent;

    private const string _gameSceneName = "OnlineGameScene";
    private const string _loadingGameText = "Starting your game...";

    private void Awake()
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

        Application.wantsToQuit += OnWantToQuit;

        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
        LobbyManager = new LobbyManager();

        AuthenticatePlayer();

        QueryLobbies();
    }

    void AuthenticatePlayer()
    {
        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var randomName = RandomNameGenerator.GetName();

        _localUser.ID.Value = localId;

        SetLocalUserName(randomName);
    }

    private void OnPlayersReady(int readyCount)
    {
        if (readyCount == _localLobby.PlayerCount &&
            _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
        {
            _localLobby.LocalLobbyState.Value = LobbyState.CountDown;
            SendLocalLobbyData();
        }
        else if (_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
        {
            _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
            SendLocalLobbyData();
        }
    }

    private void OnLobbyStateChanged(LobbyState state)
    {
        switch (state)
        {
            case LobbyState.Lobby: PlayerNotReadyEvent?.Invoke(); break;
            case LobbyState.CountDown: AllPlayersReadyEvent?.Invoke(); break;
            case LobbyState.InGame: StartGame(); break;
        }
    }

    private void StartGame()
    {
        LoadingPanel.Instance.Show(_loadingGameText);

        SetLocalUserStatus(PlayerStatus.InGame);

        if (_localUser.IsHost.Value)
        {
            _localLobby.Locked.Value = true;
            SendLocalLobbyData();
        }

        ChangeScene();

    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    #region Local Player Setters

    public void SetLocalUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.Log("Empty Name not allowed.");
            return;
        }
        _localUser.DisplayName.Value = name;
        SendLocalUserData();
    }

    public void SetLocalUserStatus(PlayerStatus status)
    {
        _localUser.UserStatus.Value = status;

        SendLocalUserData();
    }

    private async void SendLocalUserData()
    {
        await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
    }

    #endregion

    #region Local Lobby Setters

    public void SetLocalLobbyLeader(string id)
    {
        _localLobby.LeaderID.Value = id;

        SendLocalLobbyData();
    }

    private async void SendLocalLobbyData()
    {
        await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
    }

    #endregion

    #region Lobby Commands

    public async Task CreateLobby(string name, bool isPrivate, int maxPlayers = 4)
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
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async Task JoinLobby(string lobbyID, string lobbyCode)
    {
        try
        {
            var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode, _localUser);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);

            await JoinLobby();

            SetLocalUserStatus(PlayerStatus.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task CreateLobby()
    {
        _localUser.IsHost.Value = true;
        _localLobby.onUserReadyChange += OnPlayersReady;
        try
        {
            await BindLobby();

            SetLocalUserStatus(PlayerStatus.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task JoinLobby()
    {
        _localUser.IsHost.ForceSet(false);
        await BindLobby();
    }

    private async Task BindLobby()
    {
        await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);

        _localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        //_localLobby.CurrentPlayerID.onChanged += OnCurrentTurnIDChanged;

        // _localLobby.onUserTurnChanged += OnAnyUserTurnChanged;

        //SetLobbyView();
    }

    public async void LeaveLobby()
    {
        if (_localLobby == null) return;

        await LobbyManager.LeaveLobbyAsync();

        _localUser.ResetState();
        _localLobby.ResetLobby();

        LobbyList.Clear();
    }

    #endregion

    #region Lobby List Commands

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

    private void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
    {
        var newLobbyDict = new Dictionary<string, LocalLobby>();
        foreach (var lobby in lobbies)
            newLobbyDict.Add(lobby.LobbyID.Value, lobby);

        LobbyList.CurrentLobbies = newLobbyDict;
        LobbyList.QueryState.Value = LobbyQueryState.Fetched;
    }

    #endregion

    #region Teardown

    bool OnWantToQuit()
    {
        bool canQuit = string.IsNullOrEmpty(_localLobby?.LobbyID.Value);
        StartCoroutine(LeaveBeforeQuit());
        return canQuit;
    }

    IEnumerator LeaveBeforeQuit()
    {
        ForceLeaveAttempt();
        yield return null;
        Application.Quit();
    }

    void OnDestroy()
    {
        ForceLeaveAttempt();
        LobbyManager?.Dispose();
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
