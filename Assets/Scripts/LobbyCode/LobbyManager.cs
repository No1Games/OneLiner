using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
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

    private float _heartbeatTimer;

    private Lobby _joinedLobby;
    public Lobby JoinedLobby => _joinedLobby;

    LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
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

    #endregion

    public async Task<QueryResponse> GetLobbyListAsync()
    {
        //if (m_QueryCooldown.TaskQueued)
        //    return null;
        //await m_QueryCooldown.QueueUntilCooldown();

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

    #region Lobby Methods

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

    public async Task<Lobby> JoinLobbyAsync(string lobbyId, string lobbyCode, LocalPlayer localUser)
    {
        //if (m_JoinCooldown.IsCoolingDown ||
        //    (lobbyId == null && lobbyCode == null))
        //{
        //    return null;
        //}
        //
        //await m_JoinCooldown.QueueUntilCooldown();

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
                _logger.Log(e.Message);
            }
        }
    }

    //public async Task JoinLobbyByCode(string code)
    //{
    //    Player player = GetPlayer();

    //    Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, new JoinLobbyByCodeOptions
    //    {
    //        Player = player
    //    });

    //    _joinedLobby = lobby;

    //    OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
    //}

    #endregion

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
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
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
