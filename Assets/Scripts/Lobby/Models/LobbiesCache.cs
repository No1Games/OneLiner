using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Purchasing;

public class LobbiesCache
{
    public event Action<Dictionary<string, LocalLobby>> OnLobbiesUpdated;

    private Dictionary<string, LocalLobby> _lobbies = new Dictionary<string, LocalLobby>();
    public Dictionary<string, LocalLobby> Lobbies => _lobbies;

    public void UpdateLobbies(List<Lobby> updatedLobbies)
    {
        // Update existing lobbies and add new ones
        foreach (var remote in updatedLobbies)
        {
            if(!_lobbies.TryGetValue(remote.Id, out var localLobby))
            {
                localLobby = new LocalLobby();
                _lobbies[remote.Id] = localLobby;
            }

            LobbyDataConverter.UpdateLocalFromRemote(remote, localLobby);
        }

        // Remove lobbies that no longer exist
        foreach(var id in _lobbies.Keys)
        {
            if(!updatedLobbies.Exists(l => l.Id == id))
            {
                _lobbies.Remove(id);
            }
        }

        OnLobbiesUpdated?.Invoke(_lobbies);
    }
}
