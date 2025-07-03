using UnityEngine;

public static class LobbyDataKeys

{
    // Data for visual appearance of the lobby in the lobbies list
    public const string ApperanceData = nameof(LocalLobby.ApperanceData);
    // ID of the leader player in the lobby (not the host, but the player who knows the word)
    public const string LeaderID = nameof(LocalLobby.LeaderID);
    // Relay code for the lobby, used for connecting to the game
    public const string RelayCode = nameof(LocalLobby.RelayCode);
    // Used to determine what is happened in the lobby right now - waiting, countdown(all players ready), or in-game(game started)
    public const string LocalLobbyState = nameof(LocalLobby.LocalLobbyState);

    // Game-specific data
    // Index of the word that players need to guess in WordsList
    public const string LeaderWord = nameof(LocalLobby.LeaderWord);
    // Indexes of used words(indexes taken from general word list after shuffling)
    public const string WordsList = nameof(LocalLobby.WordsList);
    // ID of the player who making a turn right now (guessin or drawing line)
    public const string CurrentPlayerID = nameof(LocalLobby.CurrentPlayerID);
}
