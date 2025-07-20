using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Interface for Lobby API Service. Defines methods for interacting with the lobby system.
/// </summary>
public interface ILobbyApiService
{
    Task<QueryResponse> QueryLobbiesAsync();

    Task<Lobby> CreateLobbyAsync(int maxPlayers, bool isPrivate, LocalPlayer host);
    Task<Lobby> JoinLobbyByIdAsync(string lobbyId, LocalPlayer localUser);

    Task SubscibeOnLobbyEventsAsync(string lobbyId, LobbyEventCallbacks callbacks);

    Task UpdateLobbyDataAsync(LocalLobby lobby);
    Task LeaveLobbyAsync(string lobbyId);

    Task UpdatePlayerDataAsync(LocalPlayer player, string lobbyId);
}
