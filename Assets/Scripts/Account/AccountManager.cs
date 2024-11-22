using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


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
            SaveAccount();
        }
    }
    private void InitializeStorage()
    {
        // Тимчасова реалізація: згодом заміниться логікою вибору
        dataStorage = new PlayerPrefsDataStorage();
        saveManager = new SaveManager(dataStorage);
    }
    

private void InitializeAccount(AccountData data)
    {
        // Ініціалізація акаунта (можливо, присвоєння значень у грі)
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

    public void SaveAccount()
    {
        saveManager.Save(accountData);
    }

    public AccountData GetAccountData()
    {
        return saveManager.Load();
    }
}