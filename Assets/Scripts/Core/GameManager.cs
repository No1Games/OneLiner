using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    #region NGO Fields and Props

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private NetworkGameManager _networkGameManagerPrefab;
    private NetworkGameManager _networkGameManager;

    private bool _hasConnectedViaNGO = false;

    #endregion

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

    #region Lobby Fields and Props

    public LobbyManager LobbyManager { get; private set; }

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localUser;
    private bool _doesNeedCleanup;

    public LocalPlayer LocalUser => _localUser;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    public event Action JoinLobbyEvent;

    #endregion

    [SerializeField] Countdown _countdown;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.wantsToQuit += OnWantToQuit;

        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
        LobbyManager = new LobbyManager();

        await InitializeServices();
        AuthenticatePlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "OnlineGameScene")
        {
            InitOnlineGame();
        }
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

    //Only Host needs to listen to this and change state.
    void OnPlayersReady(int readyCount)
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

    void OnLobbyStateChanged(LobbyState state)
    {
        if (state == LobbyState.Lobby)
            CancelCountDown();
        if (state == LobbyState.CountDown)
            BeginCountDown();
    }

    void BeginCountDown()
    {
        Debug.Log("Beginning Countdown.");
        _countdown.StartCountDown();
    }

    void CancelCountDown()
    {
        Debug.Log("Countdown Cancelled.");
        _countdown.CancelCountDown();
    }

    public void FinishedCountDown()
    {
        ChangeScene();
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene("OnlineGameScene");
    }

    private void InitOnlineGame()
    {
        if (_localUser.IsHost.Value)
        {
            _localUser.UserStatus.Value = PlayerStatus.InGame;
            _localLobby.LocalLobbyState.Value = LobbyState.InGame;
            _localLobby.Locked.Value = true;
            SendLocalLobbyData();
            StartNetworkGame();
        }
    }

    public void EndGame()
    {
        if (_localUser.IsHost.Value)
        {
            _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
            _localLobby.Locked.Value = false;
            SendLocalLobbyData();
        }

        // SetLobbyView();
    }

    private async void StartNetworkGame()
    {
        _networkGameManager = Instantiate(_networkGameManagerPrefab);
        _networkGameManager.Initialize(OnConnectionVerified, _localLobby.PlayerCount, OnGameBegin, OnGameEnd, _localUser);
        if (_localUser.IsHost.Value)
        {
            await SetRelayHostData();
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            await AwaitRelayCode(_localLobby);
            await SetRelayClientData();
            NetworkManager.Singleton.StartClient();
        }
    }

    async Task AwaitRelayCode(LocalLobby lobby)
    {
        string relayCode = lobby.RelayCode.Value;
        lobby.RelayCode.onChanged += (code) => relayCode = code;
        while (string.IsNullOrEmpty(relayCode))
        {
            await Task.Delay(100);
        }
    }

    async Task SetRelayHostData()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

        var allocation = await Relay.Instance.CreateAllocationAsync(_localLobby.MaxPlayerCount.Value);
        var joincode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        GameManager.Instance.HostSetRelayCode(joincode);

        bool isSecure = false;
        var endpoint = GetEndpointForAllocation(allocation.ServerEndpoints,
            allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);

        transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);
    }

    async Task SetRelayClientData()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

        var joinAllocation = await Relay.Instance.JoinAllocationAsync(_localLobby.RelayCode.Value);
        bool isSecure = false;
        var endpoint = GetEndpointForAllocation(joinAllocation.ServerEndpoints,
            joinAllocation.RelayServer.IpV4, joinAllocation.RelayServer.Port, out isSecure);

        transport.SetClientRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
            joinAllocation.AllocationIdBytes, joinAllocation.Key,
            joinAllocation.ConnectionData, joinAllocation.HostConnectionData, isSecure);
    }

    public void HostSetRelayCode(string code)
    {
        _localLobby.RelayCode.Value = code;
        SendLocalLobbyData();
    }

    NetworkEndPoint GetEndpointForAllocation(List<RelayServerEndpoint> endpoints, string ip, int port, out bool isSecure)
    {
#if ENABLE_MANAGED_UNITYTLS
        foreach (RelayServerEndpoint endpoint in endpoints)
        {
            if (endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
            {
                isSecure = true;
                return NetworkEndPoint.Parse(endpoint.Host, (ushort)endpoint.Port);
            }
        }
#endif
        isSecure = false;
        return NetworkEndPoint.Parse(ip, (ushort)port);
    }

    string AddressFromEndpoint(NetworkEndPoint endpoint)
    {
        return endpoint.Address.Split(':')[0];
    }

    void OnConnectionVerified()
    {
        _hasConnectedViaNGO = true;
    }
    public void OnGameBegin()
    {
        if (!_hasConnectedViaNGO)
        {
            // If this localPlayer hasn't successfully connected via NGO, forcibly exit the game.
            _logger.Log("Failed to join the game.");
            OnGameEnd();
        }
    }

    /// <summary>
    /// Return to the localLobby after the game, whether due to the game ending or due to a failed connection.
    /// </summary>
    public void OnGameEnd()
    {
        if (_doesNeedCleanup)
        {
            NetworkManager.Singleton.Shutdown(true);
            Destroy(_networkGameManager.transform.parent.gameObject); // Since this destroys the NetworkManager, that will kick off cleaning up networked objects.

            _localLobby.RelayCode.Value = "";
            EndGame();
            _doesNeedCleanup = false;
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
        _localLobby.onUserReadyChange = OnPlayersReady;
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
        _localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;
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

    public void SetLocalUserStatus(PlayerStatus status)
    {
        _localUser.UserStatus.Value = status;
        SendLocalUserData();
    }

    async void SendLocalUserData()
    {
        await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localUser));
    }

    async void SendLocalLobbyData()
    {
        await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
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
