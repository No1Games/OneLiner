using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDataConverter
{
    public static Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer player)
    {
        // Create a dictionary to hold the player custom data objects
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        // Add player display name
        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, player.DisplayName.Value);
        data.Add(PlayerDataKeys.DisplayName, displayNameObject);

        return data;
    }

    public static LocalPlayer RemoteToLocal(Player remote)
    {
        var id = remote.Id;
        var displayName =
            remote.Data?.ContainsKey(PlayerDataKeys.DisplayName) == true
            ? remote.Data[PlayerDataKeys.DisplayName].Value
            : default;
        var userStatus = remote.Data?.ContainsKey(PlayerDataKeys.PlayerStatus) == true
            ? (PlayerStatus)int.Parse(remote.Data[PlayerDataKeys.PlayerStatus].Value)
            : PlayerStatus.Lobby;
        var playerRole = remote.Data?.ContainsKey(PlayerDataKeys.Role) == true
            ? (PlayerRole)int.Parse(remote.Data[PlayerDataKeys.Role].Value)
            : PlayerRole.NotSetYet;

        LocalPlayer local = new LocalPlayer();
        local.PlayerId.Value = id;
        local.DisplayName.Value = displayName;
        local.Status.Value = userStatus;
        local.Role.Value = playerRole;

        return local;
    }

    public static void UpdateLocalFromRemote(Player remote, LocalPlayer local)
    {
        var id = remote.Id;
        var displayName =
            remote.Data?.ContainsKey(PlayerDataKeys.DisplayName) == true
            ? remote.Data[PlayerDataKeys.DisplayName].Value
            : default;
        var userStatus = remote.Data?.ContainsKey(PlayerDataKeys.PlayerStatus) == true
            ? (PlayerStatus)int.Parse(remote.Data[PlayerDataKeys.PlayerStatus].Value)
            : PlayerStatus.Lobby;
        var playerRole = remote.Data?.ContainsKey(PlayerDataKeys.Role) == true
            ? (PlayerRole)int.Parse(remote.Data[PlayerDataKeys.Role].Value)
            : PlayerRole.NotSetYet;

        local.PlayerId.Value = id;
        local.DisplayName.Value = displayName;
        local.Status.Value = userStatus;
        local.Role.Value = playerRole;
    }

    public static Dictionary<string, PlayerDataObject> LocalToRemotePlayerData(LocalPlayer local)
    {
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        if (local == null || string.IsNullOrEmpty(local.PlayerId.Value))
        {
            Debug.LogWarning("Error converting local player to remote data: missing local player object or local player id.");
            return data;
        }

        data.Add(PlayerDataKeys.DisplayName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, local.DisplayName.Value));
        data.Add(PlayerDataKeys.PlayerStatus, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)local.Status.Value).ToString()));
        data.Add(PlayerDataKeys.Role, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ((int)local.Status.Value).ToString()));
        data.Add(PlayerDataKeys.IsTurn, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, local.IsTurn.Value.ToString()));

        return data;
    }
}
