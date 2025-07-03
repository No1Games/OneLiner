using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class LobbyApiService : ILobbyApiService
{
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

    /// <summary>
    /// Method to create a new lobby.
    public async Task<Lobby> CreateLobbyAsync(int maxPlayers, bool isPrivate, LocalPlayer host)
    {
        if (_createCooldown.IsCoolingDown)
        {
            Debug.LogWarning("Create Lobby hit the rate limit.");
            return null;
        }
        await _createCooldown.QueueUntilCooldown();

        string hostId = AuthenticationService.Instance.PlayerId;

        // Object to hold host cosmetic data for lobby appearance in the list
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

    public Task<Lobby> JoinLobbyAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task LeaveLobbyAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<Lobby> UpdateLobbyAsync()
    {
        throw new System.NotImplementedException();
    }
}
