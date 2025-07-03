using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Flags] // Some UI elements will want to specify multiple states in which to be active, so this is Flags.
public enum LobbyState
{
    Lobby = 1,
    CountDown = 2,
    InGame = 4
}

/// <summary>
/// A local wrapper around a lobby's remote data, with additional functionality for providing that data to UI elements and tracking local player objects.
/// (The way that the Lobby service handles its data doesn't necessarily match our needs, so we need to map from that to this LocalLobby for use in the sample code.)
/// </summary>
[Serializable]
public class LocalLobby
{
    public Action<LocalPlayer> onUserJoined;
    public Action<int> onUserLeft;
    public Action<int> onUserReadyChange;
    public Action<bool> onUserTurnChanged;
    public Action onLobbyDataChanged;

    public Action<int> PlayersCountChangedEvent;

    public CallbackValue<LobbyAppearanceData> ApperanceData = new CallbackValue<LobbyAppearanceData>();

    public CallbackValue<string> LobbyID = new CallbackValue<string>();
    public CallbackValue<string> LobbyCode = new CallbackValue<string>();
    public CallbackValue<string> RelayCode = new CallbackValue<string>();
    public CallbackValue<string> LobbyName = new CallbackValue<string>();
    public CallbackValue<string> HostID = new CallbackValue<string>();
    public CallbackValue<LobbyState> LocalLobbyState = new CallbackValue<LobbyState>();
    public CallbackValue<bool> Locked = new CallbackValue<bool>();
    public CallbackValue<bool> Private = new CallbackValue<bool>();
    public CallbackValue<int> AvailableSlots = new CallbackValue<int>();
    public CallbackValue<int> MaxPlayerCount = new CallbackValue<int>();
    public CallbackValue<long> LastUpdated = new CallbackValue<long>();

    public CallbackValue<string> Mode = new CallbackValue<string>();

    public CallbackValue<string> LeaderID = new CallbackValue<string>("");
    public CallbackValue<List<int>> WordsList = new CallbackValue<List<int>>();
    public CallbackValue<int> LeaderWord = new CallbackValue<int>();
    public CallbackValue<string> CurrentPlayerID = new CallbackValue<string>("");

    public int PlayerCount => _localPlayers.Count;
    //ServerAddress m_RelayServer;

    public List<LocalPlayer> LocalPlayers => _localPlayers;
    List<LocalPlayer> _localPlayers = new List<LocalPlayer>();

    public void ResetLobby()
    {
        _localPlayers.Clear();

        LobbyName.Value = "";
        LobbyID.Value = "";
        LobbyCode.Value = "";
        Locked.Value = false;
        Private.Value = false;
        AvailableSlots.Value = 4;
        MaxPlayerCount.Value = 4;
        onUserJoined = null;
        onUserLeft = null;

        WordsList.Value = new List<int>();
        LeaderWord.Value = -1;
        CurrentPlayerID.Value = "";

        LeaderID.Value = "";
    }

    public LocalLobby()
    {
        LastUpdated.Value = DateTime.Now.ToFileTimeUtc();
        HostID.onChanged += OnHostChanged;
    }

    ~LocalLobby()
    {
        HostID.onChanged -= OnHostChanged;
    }

    private void OnHostChanged(string newHostId)
    {
        foreach (var player in _localPlayers)
        {
            player.IsHost.Value = player.ID.Value == newHostId;
        }
    }

    public LocalPlayer GetLocalPlayer(int index)
    {
        return PlayerCount > index ? _localPlayers[index] : null;
    }

    public LocalPlayer GetLocalPlayer(string id)
    {
        if (id == null || id.Equals(string.Empty))
        {
            Debug.Log("Error: empty id");
            return null;
        }

        return _localPlayers.Where(player => player.ID.Value == id).FirstOrDefault();
    }

    public void AddPlayer(LocalPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot add a null player to the lobby.");
            return;
        }
        _localPlayers.Add(player);
    }

    public void AddPlayer(int index, LocalPlayer user)
    {
        _localPlayers.Insert(index, user);
        user.PlayerStatus.onChanged += OnUserChangedStatus;
        user.IsTurn.onChanged += OnUserChangedTurn;
        onUserJoined?.Invoke(user);
        onLobbyDataChanged?.Invoke();
        Debug.Log($"Added User: {user.DisplayName.Value} - {user.ID.Value} to slot {index + 1}/{PlayerCount}");

        PlayersCountChangedEvent?.Invoke(PlayerCount);
    }

    public void RemovePlayer(int playerIndex)
    {
        _localPlayers[playerIndex].PlayerStatus.onChanged -= OnUserChangedStatus;
        _localPlayers.RemoveAt(playerIndex);

        onUserLeft?.Invoke(playerIndex);
        onLobbyDataChanged?.Invoke();
        PlayersCountChangedEvent?.Invoke(PlayerCount);
    }

    private void OnUserChangedTurn(bool value)
    {
        onUserTurnChanged?.Invoke(value);
    }

    void OnUserChangedStatus(PlayerStatus status)
    {
        int readyCount = 0;
        foreach (var player in _localPlayers)
        {
            if (player.PlayerStatus.Value == PlayerStatus.Ready)
                readyCount++;
        }

        onUserReadyChange?.Invoke(readyCount);
        onLobbyDataChanged?.Invoke();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder("Lobby : ");
        sb.AppendLine(LobbyName.Value);
        sb.Append("ID: ");
        sb.AppendLine(LobbyID.Value);
        sb.Append("Code: ");
        sb.AppendLine(LobbyCode.Value);
        sb.Append("Locked: ");
        sb.AppendLine(Locked.Value.ToString());
        sb.Append("Private: ");
        sb.AppendLine(Private.Value.ToString());
        sb.Append("AvailableSlots: ");
        sb.AppendLine(AvailableSlots.Value.ToString());
        sb.Append("Max Players: ");
        sb.AppendLine(MaxPlayerCount.Value.ToString());
        sb.Append("LocalLobbyState: ");
        sb.AppendLine(LocalLobbyState.Value.ToString());
        sb.Append("Lobby LocalLobbyState Last Edit: ");
        sb.AppendLine(new DateTime(LastUpdated.Value).ToString());
        sb.Append("RelayCode: ");
        sb.AppendLine(RelayCode.Value);
        sb.Append("HostData: ");
        sb.AppendLine(ApperanceData.Value.ToString());
        sb.Append("LeaderID: ");
        sb.AppendLine(LeaderID.Value);

        return sb.ToString();
    }
}
