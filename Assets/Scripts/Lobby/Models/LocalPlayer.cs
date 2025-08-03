using System;

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

[Serializable]
public class LocalPlayer
{
    #region Basic player data

    public CallbackValue<bool> IsHost = new CallbackValue<bool>(false);
    public CallbackValue<string> DisplayName = new CallbackValue<string>("");
    public CallbackValue<string> PlayerId = new CallbackValue<string>("");
    public CallbackValue<long> LastUpdated = new CallbackValue<long>();

    #endregion

    #region Custom player data

    public CallbackValue<PlayerStatus> Status = new CallbackValue<PlayerStatus>(PlayerStatus.None);
    public CallbackValue<int> Index = new CallbackValue<int>(0);

    public CallbackValue<int> AvatarID = new CallbackValue<int>(1001);
    public CallbackValue<int> AvatarBackID = new CallbackValue<int>(2001);
    public CallbackValue<int> NameBackID = new CallbackValue<int>(3001);

    public CallbackValue<PlayerRole> Role = new CallbackValue<PlayerRole>(PlayerRole.NotSetYet);

    // TODO: Move to RPC
    public CallbackValue<bool> IsTurn = new CallbackValue<bool>(false);

    //public DateTime LastUpdated;

    #endregion

    public void ResetState()
    {
        IsHost.Value = false;
        Status.Value = PlayerStatus.Menu;
        Role.Value = PlayerRole.NotSetYet;
    }

    #region Legacy

    /*
     public LocalPlayer(string id, int index, bool isHost = false, string displayName = default, PlayerStatus status = default, PlayerRole role = default)
    {
        PlayerId.Value = id;
        IsHost.Value = isHost;
        Index.Value = index;
        DisplayName.Value = displayName;
        PlayerStatus.Value = status;
        Role.Value = role;
    } 
    */

    /*
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"ID: {PlayerId.Value}\n");
        sb.Append($"Name: {DisplayName.Value}\n");
        sb.Append($"Role: {Role.Value}\n");
        sb.Append($"Is Turn: {IsTurn.Value}\n");
        sb.Append($"Is Host: {IsHost.Value}\n");

        return sb.ToString();
    }
    */

    #endregion
}
