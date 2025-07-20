using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyCallbacksHandlersHelper
{
    private readonly LobbyManager _lobbyManager;

    public LobbyCallbacksHandlersHelper(LobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    public LobbyEventCallbacks CreateCallbacks()
    {
        var callbacks = new LobbyEventCallbacks();

        callbacks.LobbyChanged += OnLobbyChanged;

        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnactionStateChanged;

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
        if(changes.LobbyDeleted)
        {
            Debug.Log("Lobby has been deleted.");
            await OnLobbyDeleted();
            // _localLobby.Reset();
            return;
        }
        else
        {
            changes.ApplyToLobby(_lobbyManager.JoinedLobby);
            _lobbyManager.RefreshLobby();
        }
    }

    private void OnLobbyEventConnactionStateChanged(LobbyEventConnectionState state)
    {
        Debug.Log($"Lobby ConnectionState Changed to {state}");
    }
}
