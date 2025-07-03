using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyDataConverter
{
    public static LocalLobby RemoteToLocal(Lobby remote)
    {
        LocalLobby local = new LocalLobby();

        // Basic lobby data
        local.LobbyID.Value = remote.Id;
        local.HostID.Value = remote.HostId;
        local.LobbyName.Value = remote.Name;
        local.LobbyCode.Value = remote.LobbyCode;
        local.Private.Value = remote.IsPrivate;
        local.AvailableSlots.Value = remote.AvailableSlots;
        local.MaxPlayerCount.Value = remote.MaxPlayers;
        local.LastUpdated.Value = remote.LastUpdated.ToFileTimeUtc();

        // Custom lobby data
        local.RelayCode.Value =
            remote.Data?.ContainsKey(LobbyDataKeys.RelayCode) == true ?
            remote.Data[LobbyDataKeys.RelayCode].Value : string.Empty;

        local.LocalLobbyState.Value =
            remote.Data?.ContainsKey(LobbyDataKeys.LocalLobbyState) == true ?
            (LobbyState)int.Parse(remote.Data[LobbyDataKeys.LocalLobbyState].Value) :
            LobbyState.Lobby;

        // TODO: JSON parsing
        local.ApperanceData.Value = 
            remote.Data?.ContainsKey(LobbyDataKeys.ApperanceData) == true ?
            LobbyAppearanceData.Parse(remote.Data[LobbyDataKeys.ApperanceData].Value) :
            new LobbyAppearanceData();

        // Game-specific data (probably move to rpc calls)
        local.LeaderWord.Value =
            remote.Data?.ContainsKey(LobbyDataKeys.LeaderWord) == true ?
            int.Parse(remote.Data[LobbyDataKeys.LeaderWord].Value) : -1;

        local.WordsList.Value =
            remote.Data?.ContainsKey(LobbyDataKeys.WordsList) == true ?
            ParseWordsIndexes(remote.Data[LobbyDataKeys.WordsList].Value) : new List<int>();

        local.CurrentPlayerID.Value = 
                        remote.Data?.ContainsKey(LobbyDataKeys.CurrentPlayerID) == true ?
            remote.Data[LobbyDataKeys.CurrentPlayerID].Value : string.Empty;

        local.LeaderID.Value =
            remote.Data?.ContainsKey(LobbyDataKeys.LeaderID) == true ?
            remote.Data[LobbyDataKeys.LeaderID].Value : string.Empty;

        // Players data
        foreach (var player in remote.Players)
        {
            LocalPlayer localPlayer = PlayerDataConverter.RemoteToLocal(player);
            localPlayer.IsHost.Value = local.HostID.Value == player.Id;

            local.AddPlayer(localPlayer);
        }

        return local;
    }

    public static List<int> ParseWordsIndexes(string indexes)
    {
        string[] strings = indexes.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        List<int> result = new List<int>();
        foreach (string index in strings)
        {
            result.Add(int.Parse(index));
        }
        return result;
    }
}
