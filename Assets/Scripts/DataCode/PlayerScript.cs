[System.Serializable]
public class PlayerScript
{
    public string name;
    public PlayerRole role;
    public int playerID;
    public int avatarID;
    public int avatarBackID;
    public int nameBackID;


    public PlayerScript(string name, int playerID)
    {
        this.name = name;
        this.role = PlayerRole.NotSetYet;
        this.playerID = playerID;
        this.avatarBackID = 0;
        this.avatarID = 0;
        this.nameBackID = 0;
    }

}

public enum PlayerRole
{
    Leader = 1,
    Player = 2,
    NotSetYet = default
}
