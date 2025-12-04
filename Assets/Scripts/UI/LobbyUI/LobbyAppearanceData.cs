/// <summary>
/// Class representing the appearance data for a lobby.
/// </summary>
public class LobbyAppearanceData
{
    public string Name { get; set; }
    public int NameBackground { get; set; }
    public int Avatar { get; set; }
    public int AvatarBackground { get; set; }
    // TODO: JSON serialization could be used here instead of custom To String and Parse methods
    public override string ToString()
    {
        return $"{Name}:{NameBackground}:{Avatar}:{AvatarBackground}";
    }
    // TODO: JSON serialization could be used here instead of custom parsing
    public static LobbyAppearanceData Parse(string str)
    {
        LobbyAppearanceData data = new LobbyAppearanceData();
        string[] splitStr = str.Split(':');

        data.Name = splitStr[0];
        data.NameBackground = int.Parse(splitStr[1]);
        data.Avatar = int.Parse(splitStr[2]);
        data.AvatarBackground = int.Parse(splitStr[3]);

        return data;
    }
}
