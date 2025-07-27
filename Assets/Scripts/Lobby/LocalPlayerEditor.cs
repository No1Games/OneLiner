using System.Threading.Tasks;

public class LocalPlayerEditor
{
    private readonly LobbyManager _lobbyManager;

    private bool _pendingChanges = false;

    public LocalPlayerEditor(LobbyManager lobbyManager)
    {
        _lobbyManager = lobbyManager;
    }

    public LocalPlayerEditor SetId(string playerId)
    {
        _lobbyManager.LocalPlayer.ID.Value = playerId;
        _pendingChanges = true;
        return this;
    }

    public LocalPlayerEditor SetDisplayName(string displayName)
    {
        _lobbyManager.LocalPlayer.DisplayName.Value = displayName;
        _pendingChanges = true;
        return this;
    }

    public LocalPlayerEditor SetIsHost(bool isHost)
    {
        _lobbyManager.LocalPlayer.IsHost.ForceSet(isHost);
        _pendingChanges = true;
        return this;
    }

    public LocalPlayerEditor SetRole(PlayerRole role)
    {
        _lobbyManager.LocalPlayer.Role.Value = role;
        _pendingChanges = true;
        return this;
    }

    public LocalPlayerEditor SetStatus(PlayerStatus status)
    {
        _lobbyManager.LocalPlayer.PlayerStatus.Value = status;
        _pendingChanges = true;
        return this;
    }

    public LocalPlayerEditor SetIsTurn(bool isTurn)
    {
        _lobbyManager.LocalPlayer.IsTurn.Value = isTurn;
        _pendingChanges = true;
        return this;
    }

    public async Task CommitChangesAsync()
    {
        if (_pendingChanges)
        {
            await _lobbyManager.UpdatePlayerDataAsync();
            _pendingChanges = false;
        }
    }
}
