using System.Collections.Generic;
using System.Text;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// QueryToLocalList the lobby resulting from a request into a LocalLobby for use in the game logic.
/// </summary>
public static class LobbyConverters
{
    #region Lobby Keys

    const string key_HostData = nameof(LocalLobby.HostData);

    const string key_LeaderID = nameof(LocalLobby.LeaderID);

    const string key_RelayCode = nameof(LocalLobby.RelayCode);
    const string key_LobbyState = nameof(LocalLobby.LocalLobbyState);
    const string key_LastEdit = nameof(LocalLobby.LastUpdated);
    const string key_WordsList = nameof(LocalLobby.WordsList);
    const string key_LeaderWord = nameof(LocalLobby.LeaderWord);
    const string key_CurrentPlayerID = nameof(LocalLobby.CurrentPlayerID);

    #endregion

    #region Player Keys

    const string key_Displayname = nameof(LocalPlayer.DisplayName);
    const string key_Userstatus = nameof(LocalPlayer.UserStatus);
    const string key_PlayerRole = nameof(LocalPlayer.Role);
    const string key_IsTurn = nameof(LocalPlayer.IsTurn);

    #endregion

    public static Dictionary<string, string> LocalToRemoteLobbyData(LocalLobby lobby)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add(key_RelayCode, lobby.RelayCode.Value);
        data.Add(key_LobbyState, ((int)lobby.LocalLobbyState.Value).ToString());
        data.Add(key_LastEdit, lobby.LastUpdated.Value.ToString());

        data.Add(key_LeaderWord, lobby.LeaderWord.Value.ToString());
        data.Add(key_WordsList, ListToString(lobby.WordsList.Value));
        data.Add(key_CurrentPlayerID, lobby.CurrentPlayerID.Value);

        data.Add(key_HostData, lobby.HostData.Value.ToString());

        data.Add(key_LeaderID, lobby.LeaderID.Value);

        return data;
    }

    private static string ListToString(List<int> list)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (int i in list)
        {
            stringBuilder.Append($"{i};");
        }

        return stringBuilder.ToString();
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

    public static Dictionary<string, string> LocalToRemoteUserData(LocalPlayer user)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        if (user == null || string.IsNullOrEmpty(user.ID.Value))
            return data;
        data.Add(key_Displayname, user.DisplayName.Value);
        data.Add(key_Userstatus, ((int)user.UserStatus.Value).ToString());
        data.Add(key_PlayerRole, ((int)user.Role.Value).ToString());
        data.Add(key_IsTurn, user.IsTurn.Value.ToString());
        return data;
    }

    /// <summary>
    /// Create a new LocalLobby from the content of a retrieved lobby. Its data can be copied into an existing LocalLobby for use.
    /// </summary>
    public static void RemoteToLocal(Lobby remoteLobby, LocalLobby localLobby)
    {
        if (remoteLobby == null)
        {
            Debug.Log("Remote lobby is null, cannot convert.");
            return;
        }

        if (localLobby == null)
        {
            Debug.Log("Local Lobby is null, cannot convert");
            return;
        }

        localLobby.LobbyID.Value = remoteLobby.Id;
        localLobby.HostID.Value = remoteLobby.HostId;
        localLobby.LobbyName.Value = remoteLobby.Name;
        localLobby.LobbyCode.Value = remoteLobby.LobbyCode;
        localLobby.Private.Value = remoteLobby.IsPrivate;
        localLobby.AvailableSlots.Value = remoteLobby.AvailableSlots;
        localLobby.MaxPlayerCount.Value = remoteLobby.MaxPlayers;
        localLobby.LastUpdated.Value = remoteLobby.LastUpdated.ToFileTimeUtc();

        //Custom Lobby Data Conversions
        localLobby.RelayCode.Value = remoteLobby.Data?.ContainsKey(key_RelayCode) == true
            ? remoteLobby.Data[key_RelayCode].Value
            : localLobby.RelayCode.Value;
        localLobby.LocalLobbyState.Value = remoteLobby.Data?.ContainsKey(key_LobbyState) == true
            ? (LobbyState)int.Parse(remoteLobby.Data[key_LobbyState].Value)
            : LobbyState.Lobby;
        localLobby.LeaderWord.Value = remoteLobby.Data?.ContainsKey(key_LeaderWord) == true
            ? int.Parse(remoteLobby.Data[key_LeaderWord].Value)
            : -1;
        localLobby.WordsList.Value = remoteLobby.Data?.ContainsKey(key_WordsList) == true
            ? ParseWordsIndexes(remoteLobby.Data[key_WordsList].Value)
            : new List<int>();
        localLobby.CurrentPlayerID.Value = remoteLobby.Data?.ContainsKey(key_CurrentPlayerID) == true
            ? remoteLobby.Data[key_CurrentPlayerID].Value
            : "";
        localLobby.HostData.Value = remoteLobby.Data?.ContainsKey(key_HostData) == true
            ? HostData.Parse(remoteLobby.Data[key_HostData].Value)
            : null;
        localLobby.LeaderID.Value = remoteLobby.Data?.ContainsKey(key_LeaderID) == true
            ? remoteLobby.Data[key_LeaderID].Value
            : "";

        //Custom User Data Conversions
        List<string> remotePlayerIDs = new List<string>();
        int index = 0;
        foreach (var player in remoteLobby.Players)
        {
            var id = player.Id;
            remotePlayerIDs.Add(id);
            var isHost = remoteLobby.HostId.Equals(player.Id);
            var displayName = player.Data?.ContainsKey(key_Displayname) == true
                ? player.Data[key_Displayname].Value
                : default;
            var userStatus = player.Data?.ContainsKey(key_Userstatus) == true
                ? (PlayerStatus)int.Parse(player.Data[key_Userstatus].Value)
                : PlayerStatus.Lobby;
            var playerRole = player?.Data?.ContainsKey(key_PlayerRole) == true
                ? (PlayerRole)int.Parse(player.Data[key_PlayerRole].Value)
                : PlayerRole.NotSetYet;

            LocalPlayer localPlayer = localLobby.GetLocalPlayer(index);

            if (localPlayer == null)
            {
                localPlayer = new LocalPlayer(id, index, isHost, displayName, userStatus, playerRole);
                localLobby.AddPlayer(index, localPlayer);
            }
            else
            {
                localPlayer.ID.Value = id;
                localPlayer.Index.Value = index;
                localPlayer.IsHost.Value = isHost;
                localPlayer.DisplayName.Value = displayName;
                localPlayer.UserStatus.Value = userStatus;
                localPlayer.Role.Value = playerRole;
            }

            index++;
        }
    }

    /// <summary>
    /// Create a list of new LocalLobbies from the result of a lobby list query.
    /// </summary>
    public static List<LocalLobby> QueryToLocalList(QueryResponse response)
    {
        List<LocalLobby> retLst = new List<LocalLobby>();
        foreach (var lobby in response.Results)
            retLst.Add(RemoteToNewLocal(lobby));
        return retLst;
    }

    public static LocalLobby RemoteToNewLocal(Lobby lobby)
    {
        LocalLobby data = new LocalLobby();
        RemoteToLocal(lobby, data);
        return data;
    }
}
