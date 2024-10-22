using System;

public class RandomNameGenerator
{
    private static readonly string[] _names = new string[] { "Bear", "Wolf", "Zebra", "Monkey", "Fox", "Eagle", "Parrot", "Chiken" };

    public static string GetName()
    {
        string animal = _names[UnityEngine.Random.Range(0, _names.Length)];
        string id = Guid.NewGuid().ToString().Substring(0, 5);

        return animal + id;
    }
}
