[System.Serializable]
public class PlayerScript
{
    public string name;
    public PlayerRole role;
    public int playerID;
    public int avatarID;
    public int avatarBackID;
    public int nameBackID;


    public PlayerScript(string name, int playerID = 0)
    {
        this.name = name;
        this.role = PlayerRole.NotSetYet;
        this.playerID = playerID;
        this.avatarBackID = 201;
        this.avatarID = 101;
        this.nameBackID = 301;
    }

    public void UpdatePlayerInfo(string name, int avatarID, int avatarBackId, int nameBackId)
    {
        this.name = name;
        this.avatarBackID = avatarBackId;
        this.avatarID = avatarID;
        this.nameBackID = nameBackId;
    }

}

public enum PlayerRole
{
    Leader = 1,
    Player = 2,
    NotSetYet = default
}
