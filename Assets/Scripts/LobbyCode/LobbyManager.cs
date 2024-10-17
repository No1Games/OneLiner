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
        //if (!InLobby())
        //    return;
        //
        //string playerId = AuthenticationService.Instance.PlayerId;
        //Dictionary<string, PlayerDataObject> dataCurr = new Dictionary<string, PlayerDataObject>();
        //foreach (var dataNew in data)
        //{
        //    PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
        //        value: dataNew.Value);
        //    if (dataCurr.ContainsKey(dataNew.Key))
        //        dataCurr[dataNew.Key] = dataObj;
        //    else
        //        dataCurr.Add(dataNew.Key, dataObj);
        //}
        //
        //if (m_UpdatePlayerCooldown.TaskQueued)
        //    return;
        //await m_UpdatePlayerCooldown.QueueUntilCooldown();
        //
        //UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
        //{
        //    Data = dataCurr,
        //    AllocationId = null,
        //    ConnectionInfo = null
        //};
        //m_CurrentLobby = await LobbyService.Instance.UpdatePlayerAsync(m_CurrentLobby.Id, playerId, updateOptions);
    }

}
