using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDataConverter
{
    public static Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
    {
        // Create a dictionary to hold the player custom data objects
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        // Add player display name
        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
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
        local.ID.Value = id;
        local.DisplayName.Value = displayName;
        local.PlayerStatus.Value = userStatus;
        local.Role.Value = playerRole;

        return local;
    }
}
