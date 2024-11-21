using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//public class AccountManager : MonoBehaviour
//{
//    public PlayerScript player;

//    void Awake()
//    {
        
//            if (PlayerPrefs.HasKey("PlayerName") && PlayerPrefs.HasKey("PlayerID"))
//            {

//                LoadPlayerData();
//            }
//            else
//            {

//                CreateNewPlayer();
//            }
        
        
       
//    }

//    void LoadPlayerData()
//    {
//        // «авантажуЇмо збережен≥ дан≥ з PlayerPrefs
//        string playerName = PlayerPrefs.GetString("PlayerName");
//        int playerID = PlayerPrefs.GetInt("PlayerID");
//        int avatarID = PlayerPrefs.GetInt("AvatarID");
//        int avatarBackID = PlayerPrefs.GetInt("AvatarBackID");
//        int nameBackID = PlayerPrefs.GetInt("NameBackID");

//        // ≤н≥ц≥ал≥зуЇмо гравц€
//        player = new PlayerScript(playerName, playerID);
//        player.avatarID = avatarID;
//        player.avatarBackID = avatarBackID;
//        player.nameBackID = nameBackID;
        

//        Debug.Log("Player data loaded successfully");
//    }

//    void CreateNewPlayer()
//    {
        
//        string newPlayerName = "NewPlayer";
//        int newPlayerID = UnityEngine.Random.Range(1, 1000); 

        
//        player = new PlayerScript(newPlayerName, newPlayerID);

//        // «бер≥гаЇмо дан≥ нового гравц€ в PlayerPrefs
//        SavePlayerData();



//        Debug.Log("New player created and saved");
//    }

//    public void SavePlayerData()
//    {
//        PlayerPrefs.SetString("PlayerName", player.name);
//        PlayerPrefs.SetInt("PlayerID", player.playerID);
//        PlayerPrefs.SetInt("AvatarID", player.avatarID);
//        PlayerPrefs.SetInt("AvatarBackID", player.avatarBackID);
//        PlayerPrefs.SetInt("NameBackID", player.nameBackID);
//    }
//}
public enum AccountStatus
{
    Premium = 1,
    Basic = 2,
    Offline = default
}

public class AccountManager : MonoBehaviour
{
    private IDataStorage dataStorage;
    private SaveManager saveManager;
    private AccountData accountData;
    public PlayerScript player;

    private void Start()
    {
        InitializeStorage();

        if (saveManager.HasSave())
        {
            accountData = saveManager.Load();
            InitializeAccount(accountData);
        }
        else
        {
            accountData = CreateDefaultAccount();
            SaveAccount(accountData);
        }
    }
    private void InitializeStorage()
    {
        // “имчасова реал≥зац≥€: згодом зам≥нитьс€ лог≥кою вибору
        dataStorage = new PlayerPrefsDataStorage();
        saveManager = new SaveManager(dataStorage);
    }
    

private void InitializeAccount(AccountData data)
    {
        // ≤н≥ц≥ал≥зац≥€ акаунта (можливо, присвоЇнн€ значень у гр≥)
        player = new PlayerScript(data.playerName, data.localPlayerID);
    }

    private AccountData CreateDefaultAccount()
    {
        
        return new AccountData
        {
            playerName = "Guest",
            playerID = Guid.NewGuid().ToString(),
            localPlayerID = 0,
            avatarCode = 101,
            avatarBackgroundCode = 201,
            nameBackgroundCode = 301,
            gems = 0,
            experience = 0,
            level = 1,
            accountStatus = AccountStatus.Basic,
            
        };
    }

    public void SaveAccount(AccountData data)
    {
        saveManager.Save(data);
    }

    public AccountData GetAccountData()
    {
        return saveManager.Load();
    }
}