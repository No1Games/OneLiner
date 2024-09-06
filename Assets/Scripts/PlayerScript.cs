using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerScript 
{
    public string name;
    public PlayerRole role;
    public int playerID;

    public PlayerScript(string name, PlayerRole role, int playerID)
    {
        this.name = name;
        this.role = role;
        this.playerID = playerID;
    }

}

public enum PlayerRole
{
    Zagadue, 
    Vidgadue,
    NotSetYet
}
