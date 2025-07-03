using UnityEngine;

public static class PlayerDataKeys
{
    // Name of the player
    public const string DisplayName = nameof(LocalPlayer.DisplayName);
    // Status of the player (in lobby, ready, in game)
    public const string PlayerStatus = nameof(LocalPlayer.PlayerStatus);
    // Player role - leader, player or not set yet
    public const string Role = nameof(LocalPlayer.Role);
}
