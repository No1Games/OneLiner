using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Flags]
public enum LobbyState
{
    Lobby = 1,      // лобі очікує гравців
    Countdown = 2,  // всі гравці готові, відлік до запуску гри
    InGame = 4      // лобі в грі
}

[Serializable]
public class LocalLobby : IDisposable
{
    #region Basic lobby data

    public CallbackValue<string> LobbyID = new CallbackValue<string>();
    public CallbackValue<string> HostID = new CallbackValue<string>();
    public CallbackValue<string> RelayCode = new CallbackValue<string>();
    public CallbackValue<string> LobbyName = new CallbackValue<string>();   // Назва лобі = ім'я хоста
    public CallbackValue<bool> Locked = new CallbackValue<bool>();          // locked - якщо почалась гра
    public CallbackValue<bool> Private = new CallbackValue<bool>();
    public CallbackValue<int> AvailableSlots = new CallbackValue<int>();
    public CallbackValue<int> MaxPlayerCount = new CallbackValue<int>();
    public CallbackValue<long> LastUpdated = new CallbackValue<long>();

    // public CallbackValue<string> LobbyCode = new CallbackValue<string>();   // Код для private connection, поки не використовуємо

    #endregion

    #region Custom lobby data

    // Дані для відображення у списку (фрейм, фон, аватар, ім'я хоста)
    public CallbackValue<LobbyAppearanceData> ApperanceData = new CallbackValue<LobbyAppearanceData>();
    public CallbackValue<LobbyState> LocalLobbyState = new CallbackValue<LobbyState>();

    public CallbackValue<string> LeaderID = new CallbackValue<string>("");

    // TODO: синхронізація слів та ходу через RPC
    public CallbackValue<List<int>> WordsList = new CallbackValue<List<int>>();
    public CallbackValue<int> LeaderWord = new CallbackValue<int>();
    public CallbackValue<string> CurrentPlayerID = new CallbackValue<string>("");

    // public CallbackValue<string> Mode = new CallbackValue<string>(); // поки один режим в онлайні

    #endregion

    #region Lobby events

    public Action<LocalPlayer> PlayerJoined;
    public Action<LocalPlayer> PlayerRemoved;
    public Action<bool> PlayerStatusChanged;
    public Action<int> PlayersCountChanged;

    // TODO: синхронізація полів ходу через RPC
    public Action<bool> UserTurnChanged;

    #endregion

    public int PlayerCount => _localPlayers.Count;

    List<LocalPlayer> _localPlayers = new List<LocalPlayer>();
    public List<LocalPlayer> LocalPlayers => _localPlayers;

    public LocalLobby()
    {
        LastUpdated.Value = DateTime.Now.ToFileTimeUtc();
        HostID.onChanged += OnHostChanged;
    }

    #region Dispose

    public void Dispose()
    {
        HostID.onChanged -= OnHostChanged;

        foreach (var player in _localPlayers)
        {
            UnsubscribeFromPlayerUpdates(player);
        }
    }

    ~LocalLobby()
    {
        Dispose();
    }

    #endregion

    private void OnHostChanged(string newHostId)
    {
        foreach (var player in _localPlayers)
        {
            player.IsHost.Value = player.ID.Value == newHostId;
        }
    }

    public LocalPlayer GetLocalPlayerByIndex(int index)
    {
        if (PlayerCount == 0)
        {
            Debug.LogWarning("Can't get player. No players in lobby");
            return null;
        }

        if (index < 0 || index >= _localPlayers.Count)
        {
            Debug.LogError($"Can't get player. Incorrect index: {index}");
            return null;
        }

        return _localPlayers[index];
    }

    public void AddPlayer(LocalPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot add a null player to the lobby.");
            return;
        }

        _localPlayers.Add(player);

        SubscribeOnPlayerUpdates(player);

        PlayerJoined?.Invoke(player);
        PlayersCountChanged?.Invoke(_localPlayers.Count);
    }

    public void RemovePlayer(LocalPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot remove a null player from the lobby.");
            return;
        }

        UnsubscribeFromPlayerUpdates(player);

        _localPlayers.Remove(player);

        PlayerRemoved?.Invoke(player);
        PlayersCountChanged?.Invoke(_localPlayers.Count);
    }

    #region Event Handlers

    void OnPlayerChangedStatus(PlayerStatus status)
    {
        int readyCount = _localPlayers.Count(p => p.PlayerStatus.Value == PlayerStatus.Ready);

        PlayerStatusChanged?.Invoke(readyCount == _localPlayers.Count);
    }

    // TODO: Move to RPC
    private void OnUserChangedTurn(bool value)
    {
        UserTurnChanged?.Invoke(value);
    }

    #endregion

    private void SubscribeOnPlayerUpdates(LocalPlayer player)
    {
        player.PlayerStatus.onChanged += OnPlayerChangedStatus;
        player.IsTurn.onChanged += OnUserChangedTurn;
    }

    private void UnsubscribeFromPlayerUpdates(LocalPlayer player)
    {
        player.PlayerStatus.onChanged -= OnPlayerChangedStatus;
        player.IsTurn.onChanged -= OnUserChangedTurn;
    }
}
