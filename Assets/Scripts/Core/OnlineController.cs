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

    public LobbyManager _lobbyManager;
    public LobbyManager LobbyManager => _lobbyManager;

    // Getter for lobbies list
    public LobbiesCache Lobbies => _lobbyManager.Lobbies;

    // Getter for local lobby model
    public LocalLobby LocalLobby => _lobbyManager.LocalLobby;

    // Getter for local player model
    public LocalPlayer LocalPlayer => _lobbyManager.LocalPlayer;

    public bool IsRandomLeader { get; set; } = true;

    public event Action AllPlayersReadyEvent;
    public event Action PlayerNotReadyEvent;

    public event Action PassTurnEvent;

    private const string _gameSceneName = "OnlineGameScene";
    private const string _menuSceneName = "MainMenu";

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

        // Force leave lobby on closing the app
        Application.wantsToQuit += OnWantToQuit;

        //_lobbyManager = new LobbyManager(new LobbyApiService());
        //_lobbyManager.OnLobbyCreated += OnLobbyCreated;
        //_lobbyManager.OnLobbyJoined += OnLobbyJoined;

        //await AuthenticatePlayer();
    }

    private async void Start()
    {
        _lobbyManager = new LobbyManager(new LobbyApiService());
        _lobbyManager.OnLobbyCreated += OnLobbyCreated;
        _lobbyManager.OnLobbyJoined += OnLobbyJoined;

        await AuthenticatePlayer();
        Debug.Log("Гравець авторизований");
    }

    private async Task AuthenticatePlayer()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            try
            {
                await Initializer.TryInitServices();
                await Initializer.TrySignIn();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var displayName = RandomNameGenerator.GetName();

        // Don't commit yet since player not in lobby
        _lobbyManager.LocalPlayerEditor
            .SetId(localId)
            .SetDisplayName(displayName);
    }

    private async void OnLobbyCreated(LocalLobby lobby)
    {
        SubscribeOnLobbyUpdates();

        await _lobbyManager.LocalPlayerEditor
            .SetIsHost(true)
            .SetRole(_lobbyManager.LocalLobby.LeaderID.Value == _lobbyManager.LocalPlayer.ID.Value ? PlayerRole.Leader : PlayerRole.Player)
            .SetStatus(PlayerStatus.Lobby)
            .CommitChangesAsync();
    }

    private async void OnLobbyJoined(LocalLobby lobby)
    {
        SubscribeOnLobbyUpdates();

        await _lobbyManager.LocalPlayerEditor
            .SetIsHost(false)
            .SetRole(_lobbyManager.LocalLobby.LeaderID.Value == _lobbyManager.LocalPlayer.ID.Value ? PlayerRole.Leader : PlayerRole.Player)
            .SetStatus(PlayerStatus.Lobby)
            .CommitChangesAsync();
    }

    private void SubscribeOnLobbyUpdates()
    {
        _lobbyManager.LocalLobby.onUserReadyChange += OnPlayersReady;
        _lobbyManager.LocalLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        _lobbyManager.LocalLobby.PlayersCountChangedEvent += OnPlayersCountChanged;

        // TODO: Move turn sync to the RPC
        _lobbyManager.LocalLobby.LeaderID.onChanged += OnLeaderIDChanged;
        _lobbyManager.LocalLobby.CurrentPlayerID.onChanged += OnTurnIDChanged;
        _lobbyManager.LocalLobby.onUserTurnChanged += OnPlayerPassedTurn;
    }

    // This method sets LobbyState to Countdown if all players are ready
    private async void OnPlayersReady(int readyCount)
    {
        // Only host checks it and updates LobbyState
        if (!_lobbyManager.IsHost())
        {
            return;
        }

        if (readyCount == _lobbyManager.LocalLobby.PlayerCount && _lobbyManager.LocalLobby.LocalLobbyState.Value != LobbyState.Countdown)
        {
            await _lobbyManager.LocalLobbyEditor.SetState(LobbyState.Countdown).CommitChangesAsync();
        }
        else
        {
            await _lobbyManager.LocalLobbyEditor.SetState(LobbyState.Lobby).CommitChangesAsync();
        }
    }

    // This method is sets leader to random when player count is changed (if IsRandomLeader is true)
    // It runs every time player is connected/left to ensure random leader
    // TODO: Think about improvements
    private async void OnPlayersCountChanged(int playersCount)
    {
        // Only hosts changes leader
        if (!_lobbyManager.IsHost())
        {
            return;
        }

        if (IsRandomLeader)
        {
            int leaderIndex = UnityEngine.Random.Range(0, _lobbyManager.LocalLobby.PlayerCount);

            string leaderID = _lobbyManager.LocalLobby.GetLocalPlayer(leaderIndex).ID.Value;

            await _lobbyManager.LocalLobbyEditor
                .SetLeaderId(leaderID)
                .CommitChangesAsync();
        }
    }

    // This method makes final preps in lobby before starting the game
    public async void OnCountdownFinished()
    {
        LoadingPanel.Instance.Show();

        // Only host makes changes to lobby
        if (!_lobbyManager.IsHost())
        {
            return;
        }

        // Ensure leader Id is set before game
        // Remove when not needed
        if (string.IsNullOrEmpty(_lobbyManager.LocalLobby.LeaderID.Value))
        {
            int leaderIndex = UnityEngine.Random.Range(0, _lobbyManager.LocalLobby.PlayerCount);

            string leaderID = _lobbyManager.LocalLobby.GetLocalPlayer(leaderIndex).ID.Value;

            _lobbyManager.LocalLobbyEditor.SetLeaderId(leaderID);
        }

        _lobbyManager.LocalLobbyEditor.SetState(LobbyState.InGame);

        await _lobbyManager.LocalLobbyEditor.CommitChangesAsync();
    }

    // This method reacts to changes of lobby state
    // case Lobby - Players not ready (cancel countdown I guess)
    // case Countdown - Players ready, start countdown
    // case InGame - Countdown is finished, start the game
    private async void OnLobbyStateChanged(LobbyState state)
    {
        switch (state)
        {
            case LobbyState.Lobby: PlayerNotReadyEvent?.Invoke(); break;
            case LobbyState.Countdown: AllPlayersReadyEvent?.Invoke(); break;
            case LobbyState.InGame: await StartGame(); break;
        }
    }

    // This method reacts to changes of leader id and changes player role accordingly
    private async void OnLeaderIDChanged(string id)
    {
        // If new leader id equals current player id - change role to Leader
        // Otherwise change to player (for example, if current player was previous leader)
        if (id == _lobbyManager.LocalPlayer.ID.Value)
        {
            _lobbyManager.LocalPlayerEditor.SetRole(PlayerRole.Leader);
        }
        else
        {
            _lobbyManager.LocalPlayerEditor.SetRole(PlayerRole.Player);
        }

        await _lobbyManager.LocalPlayerEditor.CommitChangesAsync();
    }

    // This method reacts to player passing the turn
    // TODO: Move all turn related logic to RPC
    private void OnPlayerPassedTurn(bool isTurn)
    {
        if (!isTurn && _lobbyManager.IsHost())
        {
            PassTurnEvent?.Invoke();
        }
    }

    // Sets player status, locks lobby and triggers scene changing
    private async Task StartGame()
    {
        await _lobbyManager.LocalPlayerEditor
            .SetStatus(PlayerStatus.InGame)
            .CommitChangesAsync();

        if (_lobbyManager.IsHost())
        {
            await _lobbyManager.LocalLobbyEditor
                .SetLocked(true)
                .CommitChangesAsync();
        }

        ChangeScene();
    }

    // Subscribs on scene loading and start load game scene
    private void ChangeScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(_gameSceneName);
    }

    // Reacts on load of new scene
    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If new scene is menu scene (left the game)
        if (scene.name == _menuSceneName)
        {
            // New player status - lobby
            await _lobbyManager.LocalPlayerEditor
                .SetStatus(PlayerStatus.Lobby)
                .CommitChangesAsync();

            // Change lobby status and unlock it for search
            // Only host changes lobby data
            if (_lobbyManager.IsHost())
            {
                await _lobbyManager.LocalLobbyEditor
                    .SetState(LobbyState.Lobby)
                    .SetLocked(false)
                    .CommitChangesAsync();
            }

            // Open room panel menu
            MainMenuManager.Instance.OpenRoomPanel(false);
        }
        else if (scene.name == _gameSceneName)
        {
            await StartNetwork();
        }

        LoadingPanel.Instance.Hide();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async Task StartNetwork()
    {
        if (_lobbyManager.IsHost())
        {
            await SetRelayHostData();
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            await AwaitRelayCode(_lobbyManager.LocalLobby);
            await SetRelayClientData();
            NetworkManager.Singleton.StartClient();
        }
    }

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

        var allocation = await Relay.Instance.CreateAllocationAsync(_lobbyManager.LocalLobby.MaxPlayerCount.Value);
        var joincode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
        await _lobbyManager.LocalLobbyEditor
            .SetRelayCode(joincode)
            .CommitChangesAsync();

        bool isSecure = false;
        var endpoint = GetEndpointForAllocation(allocation.ServerEndpoints,
            allocation.RelayServer.IpV4, allocation.RelayServer.Port, out isSecure);

        transport.SetHostRelayData(AddressFromEndpoint(endpoint), endpoint.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, isSecure);
    }

    private async Task SetRelayClientData()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponentInChildren<UnityTransport>();

        var joinAllocation = await Relay.Instance.JoinAllocationAsync(_lobbyManager.LocalLobby.RelayCode.Value);
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

    #endregion

    #region Turn Sync - TODO: Move logic to RPC

    private async void OnTurnIDChanged(string id)
    {
        if (_lobbyManager.LocalPlayer.ID.Value == id)
        {
            await _lobbyManager.LocalPlayerEditor
                .SetIsTurn(true)
                .CommitChangesAsync();
        }
    }

    #endregion

    #region Lobby Commands

    public async Task CreateLobbyAsync(bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            await _lobbyManager.CreateLobbyAsync(isPrivate, maxPlayers);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId)
    {
        try
        {
            await _lobbyManager.JoinLobbyByIdAsync(lobbyId);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async Task LeaveLobbyAsync()
    {
        try
        {
            await _lobbyManager.LeaveLobbyAsync();
        }
        catch (LobbyServiceException exception)
        {
            Debug.LogException(exception);
        }
    }

    public async void QueryLobbiesAsync()
    {
        try
        {
            await _lobbyManager.QueryLobbiesAsync();
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error querying lobbies : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    // Закінчує гру, повертає у меню
    public async Task ReturnToLobby()
    {
        Debug.Log("Return to lobby");

        LoadingPanel.Instance.Show();

        NetworkManager.Singleton.Shutdown();

        if (_lobbyManager.IsHost())
        {
            await _lobbyManager.LocalLobbyEditor
                .SetRelayCode(string.Empty)
                .CommitChangesAsync();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(_menuSceneName);
    }

    #endregion

    #region Teardown

    private bool OnWantToQuit()
    {
        StartCoroutine(LeaveBeforeQuit());
        return true;
    }

    private IEnumerator LeaveBeforeQuit()
    {
        ForceLeaveAttempt();
        yield return null;
        Application.Quit();
    }

    private async void ForceLeaveAttempt()
    {
        await _lobbyManager.LeaveLobbyAsync();
    }

    void OnDestroy()
    {
        ForceLeaveAttempt();
        _lobbyManager.OnLobbyCreated -= OnLobbyCreated;
        _lobbyManager.OnLobbyJoined -= OnLobbyJoined;
    }

    #endregion

}
