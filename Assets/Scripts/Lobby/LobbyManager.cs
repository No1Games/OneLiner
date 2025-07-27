using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : IDisposable
{
    ILobbyApiService _lobbyApiService;

    #region State Fields

    // Service to update the local lobby model
    private LocalLobbyEditor _localLobbyEditor;
    public LocalLobbyEditor LocalLobbyEditor => _localLobbyEditor;

    // Remote lobby model
    private Lobby _joinedLobby;
    public Lobby JoinedLobby => _joinedLobby;

    // Local lobby model
    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    // List of local lobbies
    private LobbiesCache _lobbies;
    public LobbiesCache Lobbies => _lobbies;

    private LocalPlayerEditor _localPlayerEditor;
    public LocalPlayerEditor LocalPlayerEditor => _localPlayerEditor;

    private LocalPlayer _localPlayer;
    public LocalPlayer LocalPlayer => _localPlayer;

    #endregion

    LobbyEventHandler _lobbiesBinder;

    public event Action<LocalLobby> OnLobbyCreated;
    public event Action<LocalLobby> OnLobbyJoined;

    public LobbyManager(ILobbyApiService lobbyApiService)
    {
        _localLobby = new LocalLobby();
        _localPlayer = new LocalPlayer();

        _localLobbyEditor = new LocalLobbyEditor(this);
        _localPlayerEditor = new LocalPlayerEditor(this);

        _lobbyApiService = lobbyApiService;
        _lobbies = new LobbiesCache();
        _lobbiesBinder = new LobbyEventHandler(this);
    }

    public async Task QueryLobbiesAsync()
    {
        try
        {
            var result = await _lobbyApiService.QueryLobbiesAsync();

            _lobbies.UpdateLobbies(result.Results);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
        }
    }

    public async Task CreateLobbyAsync(bool isPrivate, int maxPlayers)
    {
        Lobby lobby = await _lobbyApiService.CreateLobbyAsync(maxPlayers, isPrivate, _localPlayer);

        if (lobby != null)
        {
            _joinedLobby = lobby;
            _localLobby = LobbyDataConverter.RemoteToLocal(lobby);
            StartHeartbeat();

            await SubscibeOnLobbyEventsAsync();

            OnLobbyCreated?.Invoke(_localLobby);
        }
        else
        {
            Debug.LogError("Failed to create lobby.");
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId)
    {
        Lobby lobby = await _lobbyApiService.JoinLobbyByIdAsync(lobbyId, _localPlayer);

        if (lobby != null)
        {
            _joinedLobby = lobby;
            _localLobby = LobbyDataConverter.RemoteToLocal(lobby);

            await SubscibeOnLobbyEventsAsync();

            OnLobbyJoined?.Invoke(_localLobby);
        }
        else
        {
            Debug.LogError("Failed to join lobby.");
        }
    }

    public async Task LeaveLobbyAsync()
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot leave lobby, because player is not currently in lobby.");
            return;
        }

        try
        {
            await _lobbyApiService.LeaveLobbyAsync(_joinedLobby.Id);
            _localPlayer.ResetState();
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error happened when exiting lobby: {e}");
        }

        Dispose();
    }

    public async Task UpdateLobbyDataAsync()
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot update lobby data. Player not in lobby.");
            return;
        }

        try
        {
            await _lobbyApiService.UpdateLobbyDataAsync(_localLobby);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error while updating lobby data: " + ex.Message);
        }
    }

    public async Task UpdatePlayerDataAsync()
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot update player: player currently not in the lobby");
            return;
        }

        try
        {
            await _lobbyApiService.UpdatePlayerDataAsync(_localPlayer, _joinedLobby.Id);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error while updating player data: " + ex.Message);
        }
    }

    private async Task SubscibeOnLobbyEventsAsync()
    {
        var callbacks = _lobbiesBinder.CreateCallbacks();
        try
        {
            await _lobbyApiService.SubscibeOnLobbyEventsAsync(_joinedLobby.Id, callbacks);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to subscribe to lobby events: {ex.Message}");
        }
    }

    public void RefreshLobby()
    {
        LobbyDataConverter.UpdateLocalFromRemote(_joinedLobby, _localLobby);
    }

    public bool IsInLobby()
    {
        if (_joinedLobby == null)
        {
            Debug.LogWarning("LobbyManager not currently in a lobby. Did you CreateLobbyAsync or JoinLobbyAsync?");
            return false;
        }

        return true;
    }

    public bool IsHost()
    {
        if (_joinedLobby == null)
        {
            return false;
        }

        return _localLobby.HostID.Value == _localPlayer.ID.Value;
    }

    public void Dispose()
    {
        StopHeartbeat();
        _joinedLobby = null;
    }

    #region Heartbeat

    private Task _heartBeatTask;
    private CancellationTokenSource _heartbeatCts;

    private void StartHeartbeat()
    {
        StopHeartbeat(); // Ensure any previous heartbeat is stopped
        if (_joinedLobby == null)
        {
            Debug.LogWarning("Can't start heartbeat. Player is not in the lobby.");
            return;
        }

        _heartbeatCts = new CancellationTokenSource();
        _heartBeatTask = HeartBeatLoop(_heartbeatCts.Token);
    }

    private void StopHeartbeat()
    {
        if (_heartbeatCts != null)
        {
            _heartbeatCts.Cancel();
            _heartbeatCts.Dispose();
            _heartbeatCts = null;
        }
    }

    private async Task HeartBeatLoop(CancellationToken token)
    {
        while (_joinedLobby != null && !token.IsCancellationRequested)
        {
            await _lobbyApiService.SendHeartbeatPingAsync(_joinedLobby.Id);
            await Task.Delay(8000, token);
        }
    }

    #endregion

}
