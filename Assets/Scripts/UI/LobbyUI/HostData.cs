using UnityEngine;

public class HostData
{
    public string Name { get; set; }
    public int NameBackground { get; set; }
    public int Avatar { get; set; }
    public int AvatarBackground { get; set; }

    public override string ToString()
    {
        return $"{Name}:{NameBackground}:{Avatar}:{AvatarBackground}";
    }

    public static HostData Parse(string str)
    {
        HostData data = new HostData();
        string[] splitStr = str.Split(':');

        Debug.Log(splitStr[0]);

        data.Name = splitStr[0];
        data.NameBackground = int.Parse(splitStr[1]);
        data.Avatar = int.Parse(splitStr[2]);
        data.AvatarBackground = int.Parse(splitStr[3]);

        return data;
    }
}
