using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using UnityEngine;

public class LobbyEventHandler
{
    private readonly LobbyManager _lobbyManager;

    public LobbyEventHandler(LobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    public LobbyEventCallbacks CreateCallbacks()
    {
        var callbacks = new LobbyEventCallbacks();

        callbacks.LobbyChanged += OnLobbyChanged;

        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

        return callbacks;
    }

    private async Task OnLobbyDeleted()
    {
        try
        {
            await _lobbyManager.LeaveLobbyAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to leave lobby on deletion: {ex.Message}");
        }
    }

    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            Debug.Log("Lobby has been deleted.");
            await OnLobbyDeleted();
            return;
        }
        else
        {
            changes.ApplyToLobby(_lobbyManager.JoinedLobby);
            _lobbyManager.RefreshLobby();
        }
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        Debug.Log($"Lobby ConnectionState Changed to {state}");
    }
}
