using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyApiService : ILobbyApiService
{
    #region Rate Limiting

    // Rate Limits are posted here: https://docs.unity.com/lobby/rate-limits.html

    ServiceRateLimiter _queryCooldown = new ServiceRateLimiter(1, 1f);
    ServiceRateLimiter _createCooldown = new ServiceRateLimiter(2, 6f);
    ServiceRateLimiter _joinCooldown = new ServiceRateLimiter(2, 6f);
    ServiceRateLimiter _updatePlayerCooldown = new ServiceRateLimiter(5, 5f);
    ServiceRateLimiter _updateLobbyCooldown = new ServiceRateLimiter(5, 5f);
    ServiceRateLimiter _leaveLobbyOrRemovePlayer = new ServiceRateLimiter(5, 1);
    ServiceRateLimiter _heartBeatCooldown = new ServiceRateLimiter(5, 30);

    /*
    ServiceRateLimiter m_GetLobbyCooldown = new ServiceRateLimiter(1, 1f);
    ServiceRateLimiter m_DeleteLobbyCooldown = new ServiceRateLimiter(2, 1f);
    ServiceRateLimiter _quickJoinCooldown = new ServiceRateLimiter(1, 10f);
    */

    #endregion

    public async Task<QueryResponse> QueryLobbiesAsync()
    {
        // Rate limit check for query lobbies
        // Skip if the cooldown is active
        if (_queryCooldown.IsCoolingDown)
        {
            Debug.Log("Query lobbies hit the rate limit.");
            return null;
        }

        QueryLobbiesOptions options = new QueryLobbiesOptions
        {
            Count = 25,
            Filters = new List<QueryFilter>
            {
                // Only lobbies with available slots
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                // Only lobbies that does not started game yet
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "false"
                    )
            },
            // TODO: Consider improve ordering by creation date and latency
            Order = new List<QueryOrder> // Order by creation date, descending
            {
                new QueryOrder(
                    asc: true,
                    field: QueryOrder.FieldOptions.AvailableSlots)
            }
        };

        try
        {
            await _queryCooldown.QueueUntilCooldown();
            return await Lobbies.Instance.QueryLobbiesAsync(options);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to query lobbies: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred while querying lobbies: {e.Message}");
            return null;
        }
    }

    public async Task<Lobby> CreateLobbyAsync(int maxPlayers, bool isPrivate, LocalPlayer host)
    {
        string hostId = AuthenticationService.Instance.PlayerId;

        // Object to hold host cosmetics data for lobby appearance in the list
        LobbyAppearanceData appearanceData = new LobbyAppearanceData
        {
            Name = host.DisplayName.Value,
            Avatar = host.AvatarID.Value,
            AvatarBackground = host.AvatarBackID.Value,
            NameBackground = host.NameBackID.Value
        };

        // Initialize custom data for the lobby (appearance data)
        var customData = new Dictionary<string, DataObject>()
        {
            { LobbyDataKeys.ApperanceData, new DataObject(DataObject.VisibilityOptions.Public, appearanceData.ToString()) }
            // Commented because we don't have a leader when lobby is created, setting leader on game start
            // But can be needed somewhere. Ensure that is not needed and remove then.
            // { LobbyCustomDataKeys.LeaderID, new DataObject(DataObject.VisibilityOptions.Public, host.ID.Value) }
        };

        // Inirialize lobby options
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Player = new Player(id: hostId, data: PlayerDataConverter.CreateInitialPlayerData(host)),
            Data = customData
        };

        try
        {
            // Rate limit check for creating a lobby
            // Do not skip if the cooldown is active
            await _createCooldown.QueueUntilCooldown();
            // Retrun the created lobby to the caller
            return await Lobbies.Instance.CreateLobbyAsync(appearanceData.Name, maxPlayers, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred while creating the lobby: {e.Message}");
            return null;
        }
    }

    public async Task<Lobby> JoinLobbyByIdAsync(string lobbyId, LocalPlayer player)
    {
        // Validate the lobbyId
        if (lobbyId == null)
        {
            Debug.LogError("LobbyId are null. Cannot join lobby.");
            return null;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        var playerData = PlayerDataConverter.CreateInitialPlayerData(player);

        JoinLobbyByIdOptions joinOptions = new() { Player = new Player(id: playerId, data: playerData) };

        try
        {
            // Rate limit check for joining a lobby
            // Do not skip if the cooldown is active
            await _joinCooldown.QueueUntilCooldown();
            return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby by ID: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred while joining the lobby: {e.Message}");
            return null;
        }
    }

    public async Task SubscibeOnLobbyEventsAsync(string lobbyId, LobbyEventCallbacks callbacks)
    {
        try
        {
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callbacks);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to subscribe to lobby events: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred while subscribing to lobby events: {e.Message}");
        }
    }

    public async Task LeaveLobbyAsync(string lobbyId)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        await _leaveLobbyOrRemovePlayer.QueueUntilCooldown();
        await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
    }

    public async Task UpdateLobbyDataAsync(LocalLobby lobby)
    {
        Dictionary<string, DataObject> data = LobbyDataConverter.LocalToRemoteData(lobby);

        //Special Use: Get the state of the Local lobby so we can lock it from appearing in queries if it's not in the "Lobby" LocalLobbyState
        Enum.TryParse(data[LobbyDataKeys.LocalLobbyState].Value, out LobbyState lobbyState);
        bool isLocked = lobbyState != LobbyState.Lobby;

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = isLocked };

        try
        {
            await _updateLobbyCooldown.QueueUntilCooldown();
            await LobbyService.Instance.UpdateLobbyAsync(lobby.LobbyId.Value, updateOptions);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby data: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred while updating the lobby data: {e.Message}");
        }
    }

    public async Task UpdatePlayerDataAsync(LocalPlayer player, string lobbyId)
    {
        string playerId = AuthenticationService.Instance.PlayerId;

        Dictionary<string, PlayerDataObject> data = PlayerDataConverter.LocalToRemotePlayerData(player);

        UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
        {
            Data = data
        };

        try
        {
            // Rate limit check for updating player data
            // Do not skip if the cooldown is active
            await _updatePlayerCooldown.QueueUntilCooldown();
            await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SendHeartbeatPingAsync(string lobbyId)
    {
        if (_heartBeatCooldown.IsCoolingDown)
            return;

        try
        {
            await _heartBeatCooldown.QueueUntilCooldown();
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
