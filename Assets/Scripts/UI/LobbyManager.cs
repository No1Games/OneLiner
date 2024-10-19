using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Zenject;

public class LobbyManager : MonoBehaviour
{
    const string key_RelayCode = nameof(LocalLobby.RelayCode);
    const string key_LobbyState = nameof(LocalLobby.LocalLobbyState);

    const string key_Displayname = nameof(LocalPlayer.DisplayName);
    const string key_Userstatus = nameof(LocalPlayer.UserStatus);

    public static LobbyManager Instance { get; private set; }

    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    public const string KEY_PLAYER_NAME = "PlayerName";

    private float _heartbeatTimer;
    private float _lobbyPollTimer;
    private float _refreshLobbyListTimer = 5f;

    private string _playerName;
    private Lobby _joinedLobby;
    public Lobby JoinedLobby => _joinedLobby;

    public event EventHandler OnLeftLobby;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    #region Routine Handlers

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                _heartbeatTimer = heartbeatTimerMax;

                _logger.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
        }
    }

    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            _refreshLobbyListTimer -= Time.deltaTime;
            if (_refreshLobbyListTimer < 0f)
            {
                float refreshLobbyListTimerMax = 5f;
                _refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList();
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (_joinedLobby != null)
        {
            _lobbyPollTimer -= Time.deltaTime;
            if (_lobbyPollTimer < 0f)
            {
                float lobbyPollTimerMax = 1.1f;
                _lobbyPollTimer = lobbyPollTimerMax;

                try
                {
                    // Try to fetch the lobby status
                    _joinedLobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);

                    // Fire event to update listeners that the lobby has been updated
                    OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = _joinedLobby });

                    // Check if the player is still in the lobby
                    if (!IsPlayerInLobby())
                    {
                        // Player was kicked from the lobby
                        _logger.Log("Kicked from Lobby!");

                        OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = _joinedLobby });

                        // Leave the lobby as we no longer have access
                        _joinedLobby = null;
                    }
                }
                catch (LobbyServiceException ex)
                {
                    // Handle the case where the player is kicked from a private lobby
                    _logger.Log($"Error fetching lobby: {ex.Message} {ex.Reason}");

                    // If the error is due to being kicked from a private lobby, handle it
                    if (ex.Reason == LobbyExceptionReason.Forbidden)
                    {
                        _logger.Log("Kicked from Private Lobby!");

                        OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = _joinedLobby });

                        // Nullify the lobby as the player has been kicked
                        _joinedLobby = null;
                    }
                    else
                    {
                        // Handle other exceptions if necessary
                        _logger.Log($"Unhandled Lobby Exception: {ex.Message}");
                    }
                }

            }
        }
    }

    #endregion

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            _logger.Log(e.Message);
        }
    }

    #region Lobby Methods

    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = isPrivate
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        _joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

        _logger.Log($"Created Lobby {lobby.Name} {lobby.IsPrivate} with player {player.Id}");

        return _joinedLobby;
    }

    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, LocalPlayer localUser)
    {
        // TODO COOLDOWN

        //if (m_CreateCooldown.IsCoolingDown)
        //{
        //    Debug.LogWarning("Create Lobby hit the rate limit.");
        //    return null;
        //}
        //await m_CreateCooldown.QueueUntilCooldown();

        string uasId = AuthenticationService.Instance.PlayerId;

        CreateLobbyOptions createOptions = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser))
        };

        _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

        return _joinedLobby;
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                _logger.Log(e.Message);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                _joinedLobby = null;

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                _logger.Log(e.Message);
            }
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();

        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });
        }
        catch (LobbyServiceException e)
        {
            _logger.Log(e.Message);
        }

        _logger.Log($"Lobby Joined! {_joinedLobby.Id}");

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    public async Task JoinLobbyByCode(string code)
    {
        Player player = GetPlayer();

        Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, new JoinLobbyByCodeOptions
        {
            Player = player
        });

        _joinedLobby = lobby;

        OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    }

    #endregion

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, _playerName) }
        });
    }

    #region State Check Methods

    public bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (_joinedLobby != null && _joinedLobby.Players != null)
        {
            foreach (Player player in _joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    public bool InLobby()
    {
        if (_joinedLobby == null)
        {
            Debug.LogWarning("LobbyManager not currently in a lobby. Did you CreateLobbyAsync or JoinLobbyAsync?");
            return false;
        }

        return true;
    }

    #endregion

    public async Task UpdatePlayerDataAsync(Dictionary<string, string> data)
    {
        if (!InLobby())
            return;

        string msgDebug = "";

        string playerId = AuthenticationService.Instance.PlayerId;
        Dictionary<string, PlayerDataObject> dataCurr = new Dictionary<string, PlayerDataObject>();
        foreach (var dataNew in data)
        {
            PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
                value: dataNew.Value);
            if (dataCurr.ContainsKey(dataNew.Key))
                dataCurr[dataNew.Key] = dataObj;
            else
                dataCurr.Add(dataNew.Key, dataObj);

            msgDebug += $"{dataNew.Key} -- {dataObj.Value}\n";
        }

        _logger.Log(msgDebug);

        //if (m_UpdatePlayerCooldown.TaskQueued)
        //    return;
        //await m_UpdatePlayerCooldown.QueueUntilCooldown();
        //
        UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
        {
            Data = dataCurr,
            AllocationId = null,
            ConnectionInfo = null
        };

        _logger.Log($"Update Options: {updateOptions.Data["DisplayName"].Value}");

        _joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, playerId, updateOptions);
    }

    public async Task BindLocalLobbyToRemote(string lobbyID, LocalLobby localLobby)
    {
        _lobbyEventCallbacks.LobbyDeleted += async () =>
        {
            await LeaveLobbyAsync();
        };

        _lobbyEventCallbacks.DataChanged += changes =>
        {
            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = changedValue.Value.Value;

                if (changedKey == key_LobbyState)
                    localLobby.LocalLobbyState.Value = (LobbyState)int.Parse(changedValue.Value.Value);
            }
        };

        _lobbyEventCallbacks.DataAdded += changes =>
        {
            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = changedValue.Value.Value;

                if (changedKey == key_LobbyState)
                    localLobby.LocalLobbyState.Value = (LobbyState)int.Parse(changedValue.Value.Value);
            }
        };

        _lobbyEventCallbacks.DataRemoved += changes =>
        {
            foreach (var change in changes)
            {
                var changedKey = change.Key;
                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = "";
            }
        };

        _lobbyEventCallbacks.PlayerLeft += players =>
        {
            foreach (var leftPlayerIndex in players)
            {
                localLobby.RemovePlayer(leftPlayerIndex);
            }
        };

        _lobbyEventCallbacks.PlayerJoined += players =>
        {
            foreach (var playerChanges in players)
            {
                Player joinedPlayer = playerChanges.Player;

                var id = joinedPlayer.Id;
                var index = playerChanges.PlayerIndex;
                var isHost = localLobby.HostID.Value == id;

                var newPlayer = new LocalPlayer(id, index, isHost);

                foreach (var dataEntry in joinedPlayer.Data)
                {
                    var dataObject = dataEntry.Value;
                    ParseCustomPlayerData(newPlayer, dataEntry.Key, dataObject.Value);
                }

                localLobby.AddPlayer(index, newPlayer);
            }
        };

        _lobbyEventCallbacks.PlayerDataChanged += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                foreach (var playerChange in playerChanges)
                {
                    var changedValue = playerChange.Value;

                    //There are changes on some of the changes in the player list of changes
                    var playerDataObject = changedValue.Value;
                    ParseCustomPlayerData(localPlayer, playerChange.Key, playerDataObject.Value);
                }
            }
        };

        _lobbyEventCallbacks.PlayerDataAdded += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                foreach (var playerChange in playerChanges)
                {
                    var changedValue = playerChange.Value;

                    //There are changes on some of the changes in the player list of changes
                    var playerDataObject = changedValue.Value;
                    ParseCustomPlayerData(localPlayer, playerChange.Key, playerDataObject.Value);
                }
            }
        };

        _lobbyEventCallbacks.PlayerDataRemoved += changes =>
        {
            foreach (var lobbyPlayerChanges in changes)
            {
                var playerIndex = lobbyPlayerChanges.Key;
                var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                if (localPlayer == null)
                    continue;
                var playerChanges = lobbyPlayerChanges.Value;

                //There are changes on the Player
                if (playerChanges == null)
                    continue;

                foreach (var playerChange in playerChanges.Values)
                {
                    //There are changes on some of the changes in the player list of changes
                    Debug.LogWarning("This Sample does not remove Player Values currently.");
                }
            }
        };

        _lobbyEventCallbacks.LobbyChanged += async changes =>
        {
            //Lobby Fields
            if (changes.Name.Changed)
                localLobby.LobbyName.Value = changes.Name.Value;
            if (changes.HostId.Changed)
                localLobby.HostID.Value = changes.HostId.Value;
            if (changes.IsPrivate.Changed)
                localLobby.Private.Value = changes.IsPrivate.Value;
            if (changes.IsLocked.Changed)
                localLobby.Locked.Value = changes.IsLocked.Value;
            if (changes.AvailableSlots.Changed)
                localLobby.AvailableSlots.Value = changes.AvailableSlots.Value;
            if (changes.MaxPlayers.Changed)
                localLobby.MaxPlayerCount.Value = changes.MaxPlayers.Value;

            if (changes.LastUpdated.Changed)
                localLobby.LastUpdated.Value = changes.LastUpdated.Value.ToFileTimeUtc();

            //Custom Lobby Fields

            if (changes.PlayerData.Changed)
                PlayerDataChanged();

            void PlayerDataChanged()
            {
                foreach (var lobbyPlayerChanges in changes.PlayerData.Value)
                {
                    var playerIndex = lobbyPlayerChanges.Key;
                    var localPlayer = localLobby.GetLocalPlayer(playerIndex);
                    if (localPlayer == null)
                        continue;
                    var playerChanges = lobbyPlayerChanges.Value;
                    if (playerChanges.ConnectionInfoChanged.Changed)
                    {
                        var connectionInfo = playerChanges.ConnectionInfoChanged.Value;
                        Debug.Log(
                            $"ConnectionInfo for player {playerIndex} changed to {connectionInfo}");
                    }

                    if (playerChanges.LastUpdatedChanged.Changed) { }
                }
            }
        };

        _lobbyEventCallbacks.LobbyEventConnectionStateChanged += lobbyEventConnectionState =>
        {
            Debug.Log($"Lobby ConnectionState Changed to {lobbyEventConnectionState}");
        };

        _lobbyEventCallbacks.KickedFromLobby += () =>
        {
            Debug.Log("Left Lobby");
            Dispose();
        };
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, _lobbyEventCallbacks);
    }

    public async Task LeaveLobbyAsync()
    {
        //await m_LeaveLobbyOrRemovePlayer.QueueUntilCooldown();
        if (!InLobby())
            return;
        string playerId = AuthenticationService.Instance.PlayerId;

        await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
        Dispose();
    }

    void ParseCustomPlayerData(LocalPlayer player, string dataKey, string playerDataValue)
    {
        if (dataKey == key_Userstatus)
            player.UserStatus.Value = (PlayerStatus)int.Parse(playerDataValue);
        else if (dataKey == key_Displayname)
            player.DisplayName.Value = playerDataValue;
    }

    Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
    {
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
        data.Add("DisplayName", displayNameObject);
        return data;
    }

    public void Dispose()
    {
        _joinedLobby = null;
        _lobbyEventCallbacks = new LobbyEventCallbacks();
    }
}
