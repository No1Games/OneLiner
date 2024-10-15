using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public PlayerScript player;

    void Awake()
    {
        
        if (PlayerPrefs.HasKey("PlayerName") && PlayerPrefs.HasKey("PlayerID"))
        {
            
            LoadPlayerData();
        }
        else
        {
            
            CreateNewPlayer();
        }
    }

    void LoadPlayerData()
    {
        // «авантажуЇмо збережен≥ дан≥ з PlayerPrefs
        string playerName = PlayerPrefs.GetString("PlayerName");
        int playerID = PlayerPrefs.GetInt("PlayerID");
        int avatarID = PlayerPrefs.GetInt("AvatarID");
        int avatarBackID = PlayerPrefs.GetInt("AvatarBackID");
        int nameBackID = PlayerPrefs.GetInt("NameBackID");

        // ≤н≥ц≥ал≥зуЇмо гравц€
        player = new PlayerScript(playerName, playerID);
        player.avatarID = avatarID;
        player.avatarBackID = avatarBackID;
        player.nameBackID = nameBackID;
        

        Debug.Log("Player data loaded successfully");
    }

    void CreateNewPlayer()
    {
        
        string newPlayerName = "NewPlayer";
        int newPlayerID = Random.Range(1, 1000); 

        
        player = new PlayerScript(newPlayerName, newPlayerID);

        // «бер≥гаЇмо дан≥ нового гравц€ в PlayerPrefs
        PlayerPrefs.SetString("PlayerName", player.name);
        PlayerPrefs.SetInt("PlayerID", player.playerID);
        PlayerPrefs.SetInt("AvatarID", player.avatarID);
        PlayerPrefs.SetInt("AvatarBackID", player.avatarBackID);
        PlayerPrefs.SetInt("NameBackID", player.nameBackID);
        

        Debug.Log("New player created and saved");
    }
}
