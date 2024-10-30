using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : IDisposable
{
    const string key_RelayCode = nameof(LocalLobby.RelayCode);
    const string key_LobbyState = nameof(LocalLobby.LocalLobbyState);
    const string key_LeaderWord = nameof(LocalLobby.LeaderWord);
    const string key_WordsList = nameof(LocalLobby.WordsList);

    const string key_Displayname = nameof(LocalPlayer.DisplayName);
    const string key_Userstatus = nameof(LocalPlayer.UserStatus);

    private Task _heartBeatTask;

    private Lobby _joinedLobby;
    public Lobby JoinedLobby => _joinedLobby;

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
        if (!InLobby())
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

    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, LocalPlayer localUser)
    {
        if (_createCooldown.IsCoolingDown)
        {
            Debug.LogWarning("Create Lobby hit the rate limit.");
            return null;
        }
        await _createCooldown.QueueUntilCooldown();

        string uasId = AuthenticationService.Instance.PlayerId;

        CreateLobbyOptions createOptions = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser))
        };

        _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        StartHeartBeat();

        return _joinedLobby;
    }

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

    public bool IsLobbyHost()
    {
        return _joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
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

    public async Task UpdatePlayerDataAsync(Dictionary<string, string> data)
    {
        if (!InLobby())
            return;

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

                if (changedKey == key_LeaderWord)
                    localLobby.LeaderWord.Value = int.Parse(changedValue.Value.Value);

                if (changedKey == key_WordsList)
                    localLobby.WordsList.Value = LobbyConverters.ParseWordsIndexes(changedValue.Value.Value);

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

        _lobbyEventCallbacks.LobbyChanged += changes =>
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
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            Dispose();
        };

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyID, _lobbyEventCallbacks);
    }

    public async Task LeaveLobbyAsync()
    {
        await _leaveLobbyOrRemovePlayer.QueueUntilCooldown();

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

    public async Task UpdateLobbyDataAsync(Dictionary<string, string> data)
    {
        if (!InLobby())
            return;

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
