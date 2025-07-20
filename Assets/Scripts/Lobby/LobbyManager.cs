using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : IDisposable
{
    #region Lobby Keys

    const string key_HostData = nameof(LocalLobby.ApperanceData);
    const string key_LeaderID = nameof(LocalLobby.LeaderID);

    const string key_RelayCode = nameof(LocalLobby.RelayCode);
    const string key_LobbyState = nameof(LocalLobby.LocalLobbyState);
    const string key_LeaderWord = nameof(LocalLobby.LeaderWord);
    const string key_WordsList = nameof(LocalLobby.WordsList);
    const string key_CurrentPlayerID = nameof(LocalLobby.CurrentPlayerID);

    #endregion

    #region Player Keys

    const string key_Displayname = nameof(LocalPlayer.DisplayName);
    const string key_Userstatus = nameof(LocalPlayer.PlayerStatus);
    const string key_PlayerRole = nameof(LocalPlayer.Role);
    const string key_IsTurn = nameof(LocalPlayer.IsTurn);

    #endregion

    private Task _heartBeatTask;

    LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();

    #region Rate Limiting

    public enum RequestType
    {
        Query = 0,
        Join,
        QuickJoin,
        Host
    }

    public ServiceRateLimiter GetRateLimit(RequestType type)
    {
        if (type == RequestType.Join)
            return _joinCooldown;
        else if (type == RequestType.QuickJoin)
            return _quickJoinCooldown;
        else if (type == RequestType.Host)
            return _createCooldown;
        return _queryCooldown;
    }

    // Rate Limits are posted here: https://docs.unity.com/lobby/rate-limits.html

    ServiceRateLimiter _queryCooldown = new ServiceRateLimiter(1, 1f);
    ServiceRateLimiter _createCooldown = new ServiceRateLimiter(2, 6f);
    ServiceRateLimiter _joinCooldown = new ServiceRateLimiter(2, 6f);
    ServiceRateLimiter _quickJoinCooldown = new ServiceRateLimiter(1, 10f);
    ServiceRateLimiter m_GetLobbyCooldown = new ServiceRateLimiter(1, 1f);
    ServiceRateLimiter m_DeleteLobbyCooldown = new ServiceRateLimiter(2, 1f);
    ServiceRateLimiter _updateLobbyCooldown = new ServiceRateLimiter(5, 5f);
    ServiceRateLimiter _updatePlayerCooldown = new ServiceRateLimiter(5, 5f);
    ServiceRateLimiter _leaveLobbyOrRemovePlayer = new ServiceRateLimiter(5, 1);
    ServiceRateLimiter _heartBeatCooldown = new ServiceRateLimiter(5, 30);

    #endregion

    #region HeartBeat

    async Task SendHeartbeatPingAsync()
    {
        if (!IsInLobby())
            return;
        if (_heartBeatCooldown.IsCoolingDown)
            return;
        await _heartBeatCooldown.QueueUntilCooldown();

        Debug.Log("HeartBeat");

        await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
    }

    void StartHeartBeat()
    {
        Debug.Log("Start HeartBeat");
#pragma warning disable 4014
        _heartBeatTask = HeartBeatLoop();
#pragma warning restore 4014
    }

    async Task HeartBeatLoop()
    {
        while (_joinedLobby != null)
        {
            await SendHeartbeatPingAsync();
            await Task.Delay(8000);
        }
    }

    #endregion

    ILobbyApiService _lobbyApiService;

    #region State Fields

    // Remote lobby model
    private Lobby _joinedLobby;
    public Lobby JoinedLobby => _joinedLobby;

    // Local lobby model
    private LocalLobby _localLobby;
    //public LocalLobby LocalLobby => _localLobby;

    // List of local lobbies
    private LobbiesCache _lobbies;
    public LobbiesCache Lobbies => _lobbies;

    #endregion

    LobbyCallbacksHandlersHelper _lobbiesBinder;

    public event Func<LocalLobby, Task> OnLobbyCreated;
    public event Func<LocalLobby, Task> OnLobbyJoined;

    public LobbyManager(ILobbyApiService lobbyApiService)
    {
        _lobbyApiService = lobbyApiService;
        _lobbies = new LobbiesCache();
        _lobbiesBinder = new LobbyCallbacksHandlersHelper(this);
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

    public async Task CreateLobbyAsync(bool isPrivate, int maxPlayers, LocalPlayer host)
    {
        Lobby lobby = await _lobbyApiService.CreateLobbyAsync(maxPlayers, isPrivate, host);

        if(lobby != null)
        {
            _joinedLobby = lobby;
            _localLobby = LobbyDataConverter.RemoteToLocal(lobby);
            StartHeartBeat();

            await SubscibeOnLobbyEventsAsync();

            OnLobbyCreated?.Invoke(_localLobby);
        }
        else
        {
            Debug.LogError("Failed to create lobby.");
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId, LocalPlayer player)
    {
        Lobby lobby = await _lobbyApiService.JoinLobbyByIdAsync(lobbyId, player);

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
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error happened when exiting lobby: {e}");
        }

        Dispose();
    }

    public async Task UpdateLobbyDataAsync(LocalLobby lobby)
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot update lobby data. Player not in lobby.");
            return;
        }

        try
        {
            await _lobbyApiService.UpdateLobbyDataAsync(lobby);
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error while updating lobby data: " + ex.Message);
        }
    }

    public async Task UpdatePlayerDataAsync(LocalPlayer player)
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot update player: player currently not in the lobby");
            return;
        }

        try
        {
            await _lobbyApiService.UpdatePlayerDataAsync(player, _joinedLobby.Id);
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

    public void Dispose()
    {
        _joinedLobby = null;
        _lobbyEventCallbacks = new LobbyEventCallbacks();
    }

    #region Legacy Methods

    /*
    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, LocalPlayer localUser)
    {
        if (_createCooldown.IsCoolingDown)
        {
            Debug.LogWarning("Create Lobby hit the rate limit.");
            return null;
        }
        await _createCooldown.QueueUntilCooldown();

        string uasId = AuthenticationService.Instance.PlayerId;

        LobbyAppearanceData hostData = new()
        {
            Name = localUser.DisplayName.Value,
            Avatar = localUser.AvatarID.Value,
            AvatarBackground = localUser.AvatarBackID.Value,
            NameBackground = localUser.NameBackID.Value
        };

        var customData = new Dictionary<string, DataObject>()
        {
            { key_HostData, new DataObject(DataObject.VisibilityOptions.Public, hostData.ToString()) },
            { key_LeaderID, new DataObject(DataObject.VisibilityOptions.Public, localUser.ID.Value) }
        };

        CreateLobbyOptions createOptions = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser)),
            Data = customData
        };

        _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        StartHeartBeat();

        return _joinedLobby;
    }
    */

    /*
    public async Task<Lobby> JoinLobbyAsync(string lobbyId, string lobbyCode, LocalPlayer localUser)
    {
        if (_joinCooldown.IsCoolingDown ||
            (lobbyId == null && lobbyCode == null))
        {
            return null;
        }

        await _joinCooldown.QueueUntilCooldown();

        string uasId = AuthenticationService.Instance.PlayerId;
        var playerData = CreateInitialPlayerData(localUser);

        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
            { Player = new Player(id: uasId, data: playerData) };
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }
        else
        {
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions
            { Player = new Player(id: uasId, data: playerData) };
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
        }

        return _joinedLobby;
    }
    */

    /*
    public async Task<QueryResponse> GetLobbyListAsync()
    {
        if (_queryCooldown.TaskQueued)
            return null;
        await _queryCooldown.QueueUntilCooldown();

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

        return await LobbyService.Instance.QueryLobbiesAsync(options);
    }
    */

    /*
    Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
    {
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
        data.Add("DisplayName", displayNameObject);

        return data;
    }
    */

    /*
    public async Task UpdatePlayerDataAsync(Dictionary<string, string> data, string id = null)
    {
        if (!IsInLobby())
            return;

        string playerId = id == null ? AuthenticationService.Instance.PlayerId : id;

        Dictionary<string, PlayerDataObject> dataCurr = new Dictionary<string, PlayerDataObject>();

        foreach (var dataNew in data)
        {
            PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
                value: dataNew.Value);
            if (dataCurr.ContainsKey(dataNew.Key))
                dataCurr[dataNew.Key] = dataObj;
            else
                dataCurr.Add(dataNew.Key, dataObj);
        }

        if (_updatePlayerCooldown.TaskQueued)
            return;
        await _updatePlayerCooldown.QueueUntilCooldown();

        UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
        {
            Data = dataCurr,
            AllocationId = null,
            ConnectionInfo = null
        };

        try
        {
            _joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, playerId, updateOptions);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    */

    public async Task BindLocalLobbyToRemote(string lobbyID, LocalLobby localLobby)
    {
        _lobbyEventCallbacks.LobbyDeleted += async () =>
        {
            await LeaveLobbyAsync();
        };

        _lobbyEventCallbacks.DataChanged += changes =>
        {
            if (changes == null) return;

            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                ParseCustomLobbyData(localLobby, changedValue, changedKey);
            }
        };

        _lobbyEventCallbacks.DataAdded += changes =>
        {
            foreach (var change in changes)
            {
                var changedValue = change.Value;
                var changedKey = change.Key;

                ParseCustomLobbyData(localLobby, changedValue, changedKey);
            }
        };

        _lobbyEventCallbacks.DataRemoved += changes =>
        {
            if (changes == null) return;

            foreach (var change in changes)
            {
                var changedKey = change.Key;
                if (changedKey == key_RelayCode)
                    localLobby.RelayCode.Value = "";

                if (changedKey == key_LeaderWord)
                    localLobby.LeaderWord.Value = -1;

                if (changedKey == key_WordsList)
                    localLobby.WordsList.Value = new List<int>();

                if (changedKey == key_CurrentPlayerID)
                    localLobby.CurrentPlayerID.Value = "";

                if (changedKey == key_HostData)
                    localLobby.ApperanceData.Value = null;
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
            if (changes.Data.Changed)
            {
                foreach (var change in changes.Data.Value)
                {
                    var changedValue = change.Value;
                    var changedKey = change.Key;

                    ParseCustomLobbyData(localLobby, changedValue, changedKey);
                }
            }

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

                    if (playerChanges.ChangedData.Changed)
                    {
                        foreach (var data in playerChanges.ChangedData.Value)
                        {
                            var changedValue = data.Value;

                            var playerDataObject = changedValue.Value;
                            ParseCustomPlayerData(localPlayer, data.Key, playerDataObject.Value);
                        }
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
            MainMenuManager.Instance.ChangeMenu(MenuName.RoomsList);
            Dispose();
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, _lobbyEventCallbacks);
    }

    private static void ParseCustomLobbyData(LocalLobby localLobby, ChangedOrRemovedLobbyValue<DataObject> changedValue, string changedKey)
    {
        if (changedKey == key_LeaderWord)
            localLobby.LeaderWord.Value = int.Parse(changedValue.Value.Value);

        if (changedKey == key_WordsList)
            localLobby.WordsList.Value = LobbyConverters.ParseWordsIndexes(changedValue.Value.Value);

        if (changedKey == key_RelayCode)
            localLobby.RelayCode.Value = changedValue.Value.Value;

        if (changedKey == key_LobbyState)
            localLobby.LocalLobbyState.Value = (LobbyState)int.Parse(changedValue.Value.Value);

        if (changedKey == key_CurrentPlayerID)
            localLobby.CurrentPlayerID.Value = changedValue.Value.Value;

        if (changedKey == key_HostData)
            localLobby.ApperanceData.Value = LobbyAppearanceData.Parse(changedValue.Value.Value);

        if (changedKey == key_LeaderID)
            localLobby.LeaderID.Value = changedValue.Value.Value;
    }

    void ParseCustomPlayerData(LocalPlayer player, string dataKey, string playerDataValue)
    {
        if (dataKey == key_Userstatus)
            player.PlayerStatus.Value = (PlayerStatus)int.Parse(playerDataValue);
        else if (dataKey == key_Displayname)
            player.DisplayName.Value = playerDataValue;
        else if (dataKey == key_PlayerRole)
            player.Role.Value = (PlayerRole)int.Parse(playerDataValue);
        else if (dataKey == key_IsTurn)
            player.IsTurn.Value = bool.Parse(playerDataValue);
    }

    #endregion

    #region Not Implemented Methods

    /*
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
                Debug.Log(e.Message);
            }
        }
    }
    */

    #endregion

    public bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }


    public async Task UpdateLobbyDataAsync(Dictionary<string, string> data)
    {
        if (!IsInLobby())
        {
            Debug.LogWarning("Cannot update lobby data. Player not in lobby.");
            return;
        }

        Dictionary<string, DataObject> dataCurr = _joinedLobby.Data ?? new Dictionary<string, DataObject>();

        var shouldLock = false;
        foreach (var dataNew in data)
        {
            DataObject dataObj = new DataObject(DataObject.VisibilityOptions.Public, dataNew.Value);
            if (dataCurr.ContainsKey(dataNew.Key))
                dataCurr[dataNew.Key] = dataObj;
            else
                dataCurr.Add(dataNew.Key, dataObj);

            //Special Use: Get the state of the Local lobby so we can lock it from appearing in queries if it's not in the "Lobby" LocalLobbyState
            if (dataNew.Key == "LocalLobbyState")
            {
                Enum.TryParse(dataNew.Value, out LobbyState lobbyState);
                shouldLock = lobbyState != LobbyState.Lobby;
            }
        }

        //We can still update the latest data to send to the service, but we will not send multiple UpdateLobbySyncCalls
        if (_updateLobbyCooldown.TaskQueued)
            return;
        await _updateLobbyCooldown.QueueUntilCooldown();

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = dataCurr, IsLocked = shouldLock };
        _joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, updateOptions);
    }
}
