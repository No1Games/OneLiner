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
    static OnlineController _instance;
    public static OnlineController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<OnlineController>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("OnlineGameManager");
                    _instance = singletonObj.AddComponent<OnlineController>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    #region Lobby Fields and Props

    public LobbyManager LobbyManager { get; private set; }

    private LocalLobby m_LocalLobby;
    public LocalLobby LocalLobby => m_LocalLobby;

    private LocalPlayer m_LocalPlayer;

    public LocalPlayer LocalPlayer => m_LocalPlayer;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    #endregion

    public bool IsRandomLeader { get; set; }

    private RpcHandler m_RpcHandler;

    public event Action AllPlayersReadyEvent;
    public event Action PlayerNotReadyEvent;

    public event Action PassTurnEvent;

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

        m_LocalPlayer = new LocalPlayer("", 0, false, "LocalPlayer");
        m_LocalLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
        LobbyManager = new LobbyManager();

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

        m_LocalPlayer.ID.Value = localId;

        SetLocalPlayerName(randomName);
    }

    #region Lobby Events Handlers

    private void OnPlayersReady(int readyCount)
    {
        if (!m_LocalPlayer.IsHost.Value) return;

        if (readyCount == m_LocalLobby.PlayerCount &&
            m_LocalLobby.LocalLobbyState.Value != LobbyState.CountDown)
        {
            SetLocalLobbyState(LobbyState.CountDown);
        }
        else if (m_LocalLobby.LocalLobbyState.Value == LobbyState.CountDown)
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
        if (id == m_LocalPlayer.ID.Value)
        {
            SetLocalPlayerRole(PlayerRole.Leader);
        }
        else
        {
            SetLocalPlayerRole(PlayerRole.Player);
        }
    }

    private void OnPlayersCountChanged(int playersCount)
    {
        if (!m_LocalPlayer.IsHost.Value) return;

        if (IsRandomLeader)
        {
            int leaderIndex = UnityEngine.Random.Range(0, m_LocalLobby.PlayerCount);

            string leaderID = m_LocalLobby.GetLocalPlayer(leaderIndex).ID.Value;

            SetLocalLobbyLeader(leaderID);
        }
    }

    #endregion

    #region Start Game Logic

    public void OnCountdownFinished()
    {
        LoadingPanel.Instance.Show(_loadingGameText);

        if (!m_LocalPlayer.IsHost.Value) return;

        if (string.IsNullOrEmpty(m_LocalLobby.LeaderID.Value))
        {
            int leaderIndex = UnityEngine.Random.Range(0, m_LocalLobby.PlayerCount);

            string leaderID = m_LocalLobby.GetLocalPlayer(leaderIndex).ID.Value;

            SetLocalLobbyLeader(leaderID);
        }

        SetLocalLobbyState(LobbyState.InGame);
    }

    private async void StartGame()
    {
        SetLocalPlayerStatus(PlayerStatus.InGame);

        if (m_LocalPlayer.IsHost.Value)
        {
            m_LocalLobby.Locked.Value = true;

            await SendLocalLobbyDataAsync();
        }

        ChangeScene();

        await StartNetwork();

        LoadingPanel.Instance.Hide();
    }

    private async Task StartNetwork()
    {
        m_RpcHandler = FindAnyObjectByType<RpcHandler>();

        if (m_LocalPlayer.IsHost.Value)
        {
            await SetRelayHostData();
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            await AwaitRelayCode(m_LocalLobby);
            await SetRelayClientData();
            NetworkManager.Singleton.StartClient();
        }
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(_gameSceneName);
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

        var allocation = await Relay.Instance.CreateAllocationAsync(m_LocalLobby.MaxPlayerCount.Value);
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

        var joinAllocation = await Relay.Instance.JoinAllocationAsync(m_LocalLobby.RelayCode.Value);
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
        m_LocalLobby.RelayCode.Value = code;
        await SendLocalLobbyDataAsync();
    }

    #endregion

    #region Turn Sync

    private void OnTurnIDChanged(string id)
    {
        if (m_LocalPlayer.ID.Value == id)
        {
            m_LocalPlayer.IsTurn.Value = true;

            SendLocalUserData();
        }
    }

    public async void SetTurnID(string id)
    {
        //_rpcHandler.SetTurnID(id);

        m_LocalLobby.CurrentPlayerID.Value = id;
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
        m_LocalPlayer.DisplayName.Value = name;
        SendLocalUserData();
    }

    public void SetLocalPlayerStatus(PlayerStatus status)
    {
        m_LocalPlayer.UserStatus.Value = status;

        SendLocalUserData();
    }

    public void SetLocalPlayerRole(PlayerRole role)
    {
        m_LocalPlayer.Role.Value = role;

        SendLocalUserData();
    }

    private async void SendLocalUserData()
    {
        await LobbyManager.UpdatePlayerDataAsync(LobbyConverters.LocalToRemoteUserData(m_LocalPlayer));
    }

    #endregion

    #region Local Lobby Setters

    public async void SetLocalLobbyWords(List<int> indexes, int leaderWordIndex)
    {
        m_LocalLobby.WordsList.Value = indexes;
        m_LocalLobby.LeaderWord.Value = leaderWordIndex;

        await SendLocalLobbyDataAsync();
    }

    public async void SetLocalLobbyState(LobbyState state)
    {
        m_LocalLobby.LocalLobbyState.Value = state;

        await SendLocalLobbyDataAsync();
    }

    public async void SetLocalLobbyLeader(string id)
    {
        m_LocalLobby.LeaderID.Value = id;

        await SendLocalLobbyDataAsync();
    }

    private async Task SendLocalLobbyDataAsync()
    {
        await LobbyManager.UpdateLobbyDataAsync(LobbyConverters.LocalToRemoteLobbyData(m_LocalLobby));
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
                m_LocalPlayer);

            LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);

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
            var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode, m_LocalPlayer);

            LobbyConverters.RemoteToLocal(lobby, m_LocalLobby);

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
        m_LocalPlayer.IsHost.Value = true;

        m_LocalLobby.onUserReadyChange += OnPlayersReady;

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
        m_LocalPlayer.IsHost.ForceSet(false);
        await BindLobby();
    }

    private async Task BindLobby()
    {
        await LobbyManager.BindLocalLobbyToRemote(m_LocalLobby.LobbyID.Value, m_LocalLobby);

        m_LocalLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        m_LocalLobby.PlayersCountChangedEvent += OnPlayersCountChanged;
        m_LocalLobby.PlayersCountChangedEvent += OnPlayersCountChanged;

        m_LocalLobby.LeaderID.onChanged += OnLeaderIDChanged;
        m_LocalLobby.CurrentPlayerID.onChanged += OnTurnIDChanged;

        // _localLobby.onUserTurnChanged += OnAnyUserTurnChanged;

        //SetLobbyView();
    }

    public async void LeaveLobby()
    {
        if (m_LocalLobby == null) return;

        await LobbyManager.LeaveLobbyAsync();

        m_LocalPlayer.ResetState();
        m_LocalLobby.ResetLobby();

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
        bool canQuit = string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value);
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
        if (!string.IsNullOrEmpty(m_LocalLobby?.LobbyID.Value))
        {
#pragma warning disable 4014
            LobbyManager.LeaveLobbyAsync();
#pragma warning restore 4014
            m_LocalLobby = null;
        }
    }

    #endregion
}
