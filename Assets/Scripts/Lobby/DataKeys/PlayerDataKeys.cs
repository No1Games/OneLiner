using UnityEngine;

public static class PlayerDataKeys
{
    // Name of the player
    public const string DisplayName = nameof(LocalPlayer.DisplayName);
    // Status of the player (in lobby, ready, in game)
    public const string PlayerStatus = nameof(LocalPlayer.Status);
    // Player role - leader, player or not set yet
    public const string Role = nameof(LocalPlayer.Role);
    // Is player making his move right now. Used to sync in-game turns.
    // TODO: Consider to move this sync logic to RPC from Lobby.
    public const string IsTurn = nameof(LocalPlayer.IsTurn);
}
