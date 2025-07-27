using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocalLobbyEditor
{
    private readonly LobbyManager _lobbyManager;

    private bool _pendingChanges = false;

    public LocalLobbyEditor(LobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    public LocalLobbyEditor SetState(LobbyState state)
    {
        _lobbyManager.LocalLobby.LocalLobbyState.Value = state;
        _pendingChanges = true;
        return this;
    }

    public LocalLobbyEditor SetLeaderId(string leaderID)
    {
        _lobbyManager.LocalLobby.LeaderID.Value = leaderID;
        _pendingChanges = true;
        return this;
    }

    public LocalLobbyEditor SetLocked(bool isLocked)
    {
        _lobbyManager.LocalLobby.Locked.Value = isLocked;
        _pendingChanges = true;
        return this;
    }

    public LocalLobbyEditor SetRelayCode(string relayCode)
    {
        _lobbyManager.LocalLobby.RelayCode.Value = relayCode;
        _pendingChanges = true;
        return this;
    }

    // TODO: Move words sync to RPC
    [Obsolete("Will be removed after gameplay RPC sync")]
    public LocalLobbyEditor SetWordsIndexes(List<int> indexes)
    {
        _lobbyManager.LocalLobby.WordsList.Value = indexes;
        _pendingChanges = true;
        return this;
    }

    [Obsolete("Will be removed after gameplay RPC sync")]
    public LocalLobbyEditor SetLeaderWordIndex(int wordIndex)
    {
        _lobbyManager.LocalLobby.LeaderWord.Value = wordIndex;
        _pendingChanges = true;
        return this;
    }

    // TODO: Move turn sycn to RPC
    [Obsolete("Will be removed after gameplay RPC sync")]
    public LocalLobbyEditor SetCurrentPlayerId(string id)
    {
        _lobbyManager.LocalLobby.CurrentPlayerID.Value = id;
        _pendingChanges = true;
        return this;
    }

    public async Task CommitChangesAsync()
    {
        if (_pendingChanges)
        {
            await _lobbyManager.UpdateLobbyDataAsync();
            _pendingChanges = false;
        }
    }
}
