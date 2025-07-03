using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Interface for Lobby API Service. Defines methods for interacting with the lobby system.
/// </summary>
public interface ILobbyApiService
{
    Task<Lobby> CreateLobbyAsync(int maxPlayers, bool isPrivate, LocalPlayer host);
    Task<Lobby> JoinLobbyAsync();
    Task<Lobby> UpdateLobbyAsync();
    Task LeaveLobbyAsync();
}
