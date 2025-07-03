using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineController : MonoBehaviour
{
    static OnlineController m_Instance;
    public static OnlineController Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindAnyObjectByType<OnlineController>();

                if (m_Instance == null)
                {
                    GameObject singletonObj = new GameObject("OnlineGameManager");
                    m_Instance = singletonObj.AddComponent<OnlineController>();
                }

                DontDestroyOnLoad(m_Instance.gameObject);
            }

            return m_Instance;
        }
    }

    #region Lobby Fields and Props

    public LobbyManager _lobbyManager;
    public LobbyManager LobbyManager => _lobbyManager;

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localPlayer;

    public LocalPlayer LocalPlayer => _localPlayer;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    #endregion

    public bool IsRandomLeader { get; set; }

    //private RpcHandler m_RpcHandler;

    public event Action AllPlayersReadyEvent;
    public event Action PlayerNotReadyEvent;

    public event Action PassTurnEvent;

    private const string m_GameSceneName = "OnlineGameScene";
    private const string m_MenuSceneName = "MainMenu";

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (m_Instance != this)
        {
            Destroy(gameObject);
        }

        Application.wantsToQuit += OnWantToQuit;

        _localPlayer = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };

        _lobbyManager = new LobbyManager(new LobbyApiService());
        _lobbyManager.OnLobbyCreated += OnLobbyCreated;

        AuthenticatePlayer();

        QueryLobbies();
    }

    async void AuthenticatePlayer()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await Initializer.TryInitServices();
            await Initializer.TrySignIn();
        }

        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var randomName = RandomNameGenerator.GetName();

        _localPlayer.ID.Value = localId;

        SetLocalPlayerName(randomName);
    }

    #region Lobby Events Handlers

    private async Task OnLobbyCreated(LocalLobby lobby)
    {
        _localLobby = lobby;

        await CreateLobby();
    }

    private void OnPlayersReady(int readyCount)
    {
        if (!_localPlayer.IsHost.Value) return;

        if (readyCount == _localLobby.PlayerCount &&
            _localLobby.LocalLobbyState.Value != LobbyState.CountDown)
        {
            SetLocalLobbyState(LobbyState.CountDown);
        }
        else if (_localLobby.LocalLobbyState.Value == LobbyState.CountDown)
        {
            SetLocalLobbyState(LobbyState.Lobby);
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

    private void OnLeaderIDChanged(string id)
    {
        if (id == _localPlayer.ID.Value)
        {
            SetLocalPlayerRole(PlayerRole.Leader);
        }
        else
        {
            SetLocalPlayerRole(PlayerRole.Player);
        }
    }

    private void OnPlayerPassedTurn(bool isTurn)
    {
        if (!isTurn && _localPlayer.IsHost.Value)
        {
            PassTurnEvent?.Invoke();
        }
    }

    private void OnPlayersCountChanged(int playersCount)
    {
        if (!_localPlayer.IsHost.Value) return;

        if (IsRandomLeader)
        {
            int leaderIndex = UnityEngine.Random.Range(0, _localLobby.PlayerCount);

            string leaderID = _localLobby.GetLocalPlayer(leaderIndex).ID.Value;

            SetLocalLobbyLeader(leaderID);
        }
    }

    #endregion

    #region Start Game Logic

    public void OnCountdownFinished()
    {
        LoadingPanel.Instance.Show();

        if (!_localPlayer.IsHost.Value) return;

        if (string.IsNullOrEmpty(_localLobby.LeaderID.Value))
        {
            int leaderIndex = UnityEngine.Random.Range(0, _localLobby.PlayerCount);

            string leaderID = _localLobby.GetLocalPlayer(leaderIndex).ID.Value;

            SetLocalLobbyLeader(leaderID);
        }

        SetLocalLobbyState(LobbyState.InGame);
    }

    private void StartGame()
    {
        ChangeScene();
    }

    private async Task StartNetwork()
    {
        //m_RpcHandler = FindAnyObjectByType<RpcHandler>();

        if (_localPlayer.IsHost.Value)
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

    private async void ChangeScene()
    {
        SetLocalPlayerStatus(PlayerStatus.InGame);

        if (_localPlayer.IsHost.Value)
        {
            _localLobby.Locked.Value = true;

            await SendLocalLobbyDataAsync();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(m_GameSceneName);
        //NetworkManager.Singleton.SceneManager.LoadScene(m_GameSceneName, LoadSceneMode.Additive);
    }

    #endregion

    #region Relay

    private async Task AwaitRelayCode(LocalLobby lobby)
    {
        string relayCode = lobby.RelayCode.Value;
        lobby.RelayCode.onChanged += (code) => relayCode = code;
        while (string.IsNullOrEmpty(relayCode))
        {
            await Task.Delay(100);
        }
    }

    private async Task SetRelayHostData()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

        var allocation = await Relay.Instance.CreateAllocationAsync(_localLobby.MaxPlayerCount.Value);
        var joincode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Instance.HostSetRelayCode(joincode);

        bool isSecure = false;
        var endpoint = GetEndpointForAllocation(allocation.ServerEndpoints,
            allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);

        transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);
    }

    private async Task SetRelayClientData()
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

    private NetworkEndpoint GetEndpointForAllocation(List<RelayServerEndpoint> endpoints, string ip, int port, out bool isSecure)
    {
#if ENABLE_MANAGED_UNITYTLS
        foreach (RelayServerEndpoint endpoint in endpoints)
        {
            if (endpoint.Secure && endpoint.Network == RelayServerEndpoint.NetworkOptions.Udp)
            {
                isSecure = true;
                return NetworkEndpoint.Parse(endpoint.Host, (ushort)endpoint.Port);
            }
        }
#endif
        isSecure = false;
        return NetworkEndpoint.Parse(ip, (ushort)port);
    }

    string AddressFromEndpoint(NetworkEndpoint endpoint)
    {
        return endpoint.Address.Split(':')[0];
    }

    public async void HostSetRelayCode(string code)
    {
        _localLobby.RelayCode.Value = code;
        await SendLocalLobbyDataAsync();
    }

    #endregion

    #region Turn Sync

    private void OnTurnIDChanged(string id)
    {
        if (_localPlayer.ID.Value == id)
        {
            _localPlayer.IsTurn.Value = true;

            SendLocalUserData();
        }
    }

    public async void SetTurnID(string id)
    {
        //_rpcHandler.SetTurnID(id);

        _localLobby.CurrentPlayerID.Value = id;
        await SendLocalLobbyDataAsync();
    }

    #endregion

    #region Local Player Setters

    public void SetLocalPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.Log("Empty Name not allowed.");
            return;
        }
        _localPlayer.DisplayName.Value = name;
        SendLocalUserData();
    }

    public void SetLocalPlayerStatus(PlayerStatus status)
    {
        _localPlayer.PlayerStatus.Value = status;

        SendLocalUserData();
    }

    public void SetLocalPlayerRole(PlayerRole role)
    {
        _localPlayer.Role.Value = role;

        SendLocalUserData();
    }

    public void SetLocalPlayerTurn(bool isTurn)
    {
        _localPlayer.IsTurn.Value = isTurn;

        SendLocalUserData();
    }

    private async void SendLocalUserData()
    {
        await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(_localPlayer));
    }

    #endregion

    #region Local Lobby Setters

    public async void SetLocalLobbyWords(List<int> indexes, int leaderWordIndex)
    {
        _localLobby.WordsList.Value = indexes;

        await SendLocalLobbyDataAsync();

        _localLobby.LeaderWord.Value = leaderWordIndex;

        await SendLocalLobbyDataAsync();
    }

    public async void SetLocalLobbyState(LobbyState state)
    {
        _localLobby.LocalLobbyState.Value = state;

        await SendLocalLobbyDataAsync();
    }

    public async void SetLocalLobbyLeader(string id)
    {
        _localLobby.LeaderID.Value = id;

        await SendLocalLobbyDataAsync();
    }

    private async Task SendLocalLobbyDataAsync()
    {
        await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(_localLobby));
    }

    #endregion

    #region Lobby Commands

    public async Task CreateLobbyAsync(bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            await _lobbyManager.CreateLobbyAsync(isPrivate, maxPlayers, _localPlayer);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async Task CreateLobby(string name, bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            var lobby = await LobbyManager.CreateLobbyAsync(
                name,
                maxPlayers,
                isPrivate,
                _localPlayer);

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
            var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode, _localPlayer);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);

            await JoinLobby();

            SetLocalPlayerStatus(PlayerStatus.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task CreateLobby()
    {
        _localPlayer.IsHost.Value = true;

        _localLobby.onUserReadyChange += OnPlayersReady;

        try
        {
            await BindLobby();

            SetLocalPlayerStatus(PlayerStatus.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task JoinLobby()
    {
        _localPlayer.IsHost.ForceSet(false);
        await BindLobby();
    }

    private async Task BindLobby()
    {
        await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);

        _localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        _localLobby.PlayersCountChangedEvent += OnPlayersCountChanged;
        _localLobby.PlayersCountChangedEvent += OnPlayersCountChanged;

        _localLobby.LeaderID.onChanged += OnLeaderIDChanged;
        _localLobby.CurrentPlayerID.onChanged += OnTurnIDChanged;

        if (_localLobby.LeaderID.Value == _localPlayer.ID.Value)
        {
            SetLocalPlayerRole(PlayerRole.Leader);
        }
        else
        {
            SetLocalPlayerRole(PlayerRole.Player);
        }

        _localLobby.onUserTurnChanged += OnPlayerPassedTurn;

        //SetLobbyView();
    }

    public void ReturnToLobby()
    {
        Debug.Log("Return to lobby");

        LoadingPanel.Instance.Show();

        NetworkManager.Singleton.Shutdown();

        if (_localPlayer.IsHost.Value)
        {
            HostSetRelayCode(string.Empty);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(m_MenuSceneName);
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == m_MenuSceneName)
        {
            SetLocalPlayerStatus(PlayerStatus.Lobby);

            if (_localPlayer.IsHost.Value)
            {
                _localLobby.LocalLobbyState.Value = LobbyState.Lobby;
                _localLobby.Locked.Value = false;
                await SendLocalLobbyDataAsync();
            }

            MainMenuManager.Instance.OpenRoomPanel(false);
        }

        if (scene.name == m_GameSceneName)
        {


            await StartNetwork();
        }

        LoadingPanel.Instance.Hide();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public async void LeaveLobby()
    {
        if (_localLobby == null) return;

        await LobbyManager.LeaveLobbyAsync();

        _localPlayer.ResetState();
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
        _lobbyManager.OnLobbyCreated -= OnLobbyCreated;
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
