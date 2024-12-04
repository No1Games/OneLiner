using System;
using System.Text;

/// <summary>
/// Current state of the user in the lobby.
/// This is a Flags enum to allow for the Inspector to select multiples for various UI features.
/// </summary>
[Flags]
public enum PlayerStatus
{
    None = 0,
    Connecting = 1, // User has joined a lobby but has not yet connected to Relay.
    Lobby = 2, // User is in a lobby and connected to Relay.
    Ready = 4, // User has selected the ready button, to ready for the "game" to start.
    InGame = 8, // User is part of a "game" that has started.
    Menu = 16 // User is not in a lobby, in one of the main menus.
}

/// <summary>
/// Data for a local player instance. This will update data and is observed to know when to push local player changes to the entire lobby.
/// </summary>
[Serializable]
public class LocalPlayer
{
    public CallbackValue<bool> IsHost = new CallbackValue<bool>(false);
    public CallbackValue<string> DisplayName = new CallbackValue<string>("");
    public CallbackValue<PlayerStatus> UserStatus = new CallbackValue<PlayerStatus>((PlayerStatus)0);
    public CallbackValue<string> ID = new CallbackValue<string>("");
    public CallbackValue<int> Index = new CallbackValue<int>(0);

    public CallbackValue<int> AvatarID = new CallbackValue<int>(1001);
    public CallbackValue<int> AvatarBackID = new CallbackValue<int>(2001);
    public CallbackValue<int> NameBackID = new CallbackValue<int>(3001);

    public CallbackValue<PlayerRole> Role = new CallbackValue<PlayerRole>((PlayerRole)0);

    public CallbackValue<bool> IsTurn = new CallbackValue<bool>(false);

    public DateTime LastUpdated;

    public LocalPlayer(string id, int index, bool isHost, string displayName = default, PlayerStatus status = default, PlayerRole role = default)
    {
        ID.Value = id;
        IsHost.Value = isHost;
        Index.Value = index;
        DisplayName.Value = displayName;
        UserStatus.Value = status;
        Role.Value = role;
    }

    public void ResetState()
    {
        IsHost.Value = false;
        UserStatus.Value = PlayerStatus.Menu;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"ID: {ID.Value}\n");
        sb.Append($"Name: {DisplayName.Value}\n");
        sb.Append($"Role: {Role.Value}\n");
        sb.Append($"Is Turn: {IsTurn.Value}\n");
        sb.Append($"Is Host: {IsHost.Value}\n");

        return sb.ToString();
    }
}
