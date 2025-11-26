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
            .SetStatus(PlayerStatus.Lobby)
            .CommitChangesAsync();
    }

    private async void OnLobbyJoined(LocalLobby lobby)
    {
        SubscribeOnLobbyUpdates();

        await _lobbyManager.LocalPlayerEditor
            .SetIsHost(false)
            .SetStatus(PlayerStatus.Lobby)
            .CommitChangesAsync();
    }

    private void SubscribeOnLobbyUpdates()
    {
        _lobbyManager.LocalLobby.PlayerStatusChanged += OnPlayersReady;
        _lobbyManager.LocalLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        _lobbyManager.LocalLobby.PlayersCountChanged += OnPlayersCountChanged;

        _lobbyManager.LocalLobby.LeaderID.onChanged += OnLeaderIDChanged;

        _lobbyManager.LocalLobby.HostID.onChanged += OnHostIDChanged;
    }

    private async void OnHostIDChanged(string newId)
    {
        // TODO: Host migration in game
        if (newId == _lobbyManager.LocalPlayer.PlayerId.Value)
        {
            await _lobbyManager.LocalPlayerEditor.SetIsHost(true).CommitChangesAsync();
        }
    }

    private void UnsubscibeFromLobbyUpdates()
    {
        _lobbyManager.LocalLobby.PlayerStatusChanged -= OnPlayersReady;
        _lobbyManager.LocalLobby.LocalLobbyState.onChanged -= OnLobbyStateChanged;

        _lobbyManager.LocalLobby.PlayersCountChanged -= OnPlayersCountChanged;

        _lobbyManager.LocalLobby.LeaderID.onChanged -= OnLeaderIDChanged;

        _lobbyManager.LocalLobby.HostID.onChanged -= OnHostIDChanged;
    }

    // This method sets LobbyState to Countdown if all players are ready
    private async void OnPlayersReady(bool isAllReady)
    {
        // Only host checks it and updates LobbyState
        if (!_lobbyManager.IsHost())
        {
            return;
        }

        if (isAllReady && _lobbyManager.LocalLobby.LocalLobbyState.Value != LobbyState.Countdown)
        {

            await _lobbyManager.LocalLobbyEditor.SetState(LobbyState.Countdown).SetLocked(true).CommitChangesAsync();
        }
        else
        {
            await _lobbyManager.LocalLobbyEditor.SetState(LobbyState.Lobby).SetLocked(false).CommitChangesAsync();
        }
    }

    private void OnPlayersCountChanged(int playersCount)
    {
        Debug.Log($"New player connected. New players count: {playersCount}");

        // Only hosts changes leader
        if (!_lobbyManager.IsHost())
        {
            return;
        }
    }

    // This method makes final preps in lobby before starting the game
    public async void OnCountdownFinished()
    {
        LoadingPanel.Instance.Show();

        // Only host makes changes to lobby
        if (_lobbyManager.IsHost())
        {
            // Ensure leader Id is set before game
            string leaderId = GetRandomLeaderId();

            await _lobbyManager.LocalLobbyEditor
                .SetState(LobbyState.InGame)
                .SetLeaderId(leaderId)
                .CommitChangesAsync();
        }

        await _lobbyManager.LocalPlayerEditor
            .SetStatus(PlayerStatus.InGame)
            .CommitChangesAsync();
    }

    // This method reacts to changes of lobby state
    // case Lobby - Players not ready (cancel countdown I guess)
    // case Countdown - Players ready, start countdown
    // case InGame - Countdown is finished, start the game
    private void OnLobbyStateChanged(LobbyState state)
    {
        switch (state)
        {
            case LobbyState.Lobby: PlayerNotReadyEvent?.Invoke(); break;
            case LobbyState.Countdown: AllPlayersReadyEvent?.Invoke(); break;
            case LobbyState.InGame: ChangeScene(); break;
        }
    }

    // This method reacts to changes of leader id and changes player role accordingly
    private async void OnLeaderIDChanged(string id)
    {
        // If new leader id equals current player id - change role to Leader
        // Otherwise change to player(for example, if current player was previous leader)
        if (id == _lobbyManager.LocalPlayer.PlayerId.Value)
        {
            await _lobbyManager.LocalPlayerEditor.SetRole(PlayerRole.Leader).CommitChangesAsync();
        }
        else if (_lobbyManager.LocalPlayer.Role.Value != PlayerRole.Player)
        {
            await _lobbyManager.LocalPlayerEditor.SetRole(PlayerRole.Player).CommitChangesAsync();
        }
    }

    // Subscribes on scene loading and start load game scene
    private void ChangeScene()
    {
        Debug.Log("Change Scene called");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(_gameSceneName);
    }

    // Reacts on load of new scene
    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == _gameSceneName) // If start the game
        {
            await StartNetwork();
            Debug.Log("Game Scene Loaded");
        }
        else if (scene.name == _menuSceneName) // If left the game
        {
            if (_lobbyManager.IsInLobby()) // Restart
            {
                // New player status - lobby
                _lobbyManager.LocalPlayerEditor
                    .SetStatus(PlayerStatus.Lobby);

                // Change lobby status and unlock it for search
                // Only host changes lobby data
                if (_lobbyManager.IsHost())
                {
                    _lobbyManager.LocalLobbyEditor
                        .SetState(LobbyState.Lobby)
                        .SetLocked(false);
                }

                await _lobbyManager.LocalLobbyEditor.CommitChangesAsync();

                // Open room panel menu
                MainMenuManager.Instance.OpenRoomPanel(false);
            }
            else
            {
                MainMenuManager.Instance.ChangeMenu(MenuName.RoomsList);
            }
        }

        LoadingPanel.Instance.Hide();
    }

    private async Task StartNetwork()
    {
        if (_lobbyManager.IsHost())
        {
            await SetRelayHostData();
            bool result = NetworkManager.Singleton.StartHost();
            Debug.Log($"Start host result: {result}");
        }
        else
        {
            await AwaitRelayCode(_lobbyManager.LocalLobby);
            await SetRelayClientData();
            bool result = NetworkManager.Singleton.StartClient();
            Debug.Log($"Start client result: {result}");
        }
    }

    private string GetRandomLeaderId()
    {
        int leaderIndex = UnityEngine.Random.Range(0, _lobbyManager.LocalLobby.PlayerCount);

        string leaderID = _lobbyManager.LocalLobby.GetLocalPlayerByIndex(leaderIndex).PlayerId.Value;

        return leaderID;
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
        // Get transport reference
        UnityTransport transport = GetComponent<UnityTransport>();
        // Allocate relay for players number
        var allocation = await Relay.Instance.CreateAllocationAsync(_lobbyManager.LocalLobby.MaxPlayerCount.Value);
        // Get the join code for this lobby
        var joincode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Save join code in lobby data
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
        UnityTransport transport = GetComponent<UnityTransport>();

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
            UnsubscibeFromLobbyUpdates();

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

    public async Task ReturnToLobbyList()
    {
        LoadingPanel.Instance.Show();

        NetworkManager.Singleton.Shutdown();

        await _lobbyManager.LeaveLobbyAsync();

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
        if (_lobbyManager != null && _lobbyManager.IsInLobby())
        {
            ForceLeaveAttempt();
            yield return null;
        }
        Application.Quit();
    }

    private async void ForceLeaveAttempt()
    {
        await LeaveLobbyAsync();
    }

    void OnDestroy()
    {
        if (_lobbyManager is null) return;
        ForceLeaveAttempt();
        _lobbyManager.OnLobbyCreated -= OnLobbyCreated;
        _lobbyManager.OnLobbyJoined -= OnLobbyJoined;
    }

    #endregion

}