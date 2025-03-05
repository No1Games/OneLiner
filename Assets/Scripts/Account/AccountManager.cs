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

    [Header("Default Items")]
    [SerializeField] private List<int> defaultCosmeticCodes;

    [SerializeField] private bool runWithPreviousAcc; 

    private void Awake()
    {
        InitializeStorage();

        if (runWithPreviousAcc)
        {
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
        else
        {
            accountData = CreateDefaultAccount();
            SaveAccount();
        }

        
        UpdateDefaultCosmetics();
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
        player.UpdatePlayerInfo(data.playerName, data.avatarCode, data.avatarBackgroundCode, data.nameBackgroundCode);
    }

    private AccountData CreateDefaultAccount()
    {
        
        return new AccountData
        {
            playerName = "Guest",
            playerID = Guid.NewGuid().ToString(),
            localPlayerID = 0,
            avatarCode = 1001,
            avatarBackgroundCode = 2001,
            nameBackgroundCode = 3001,
            cosmeticCodes = defaultCosmeticCodes,
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
        return accountData;
    }

    private void UpdateDefaultCosmetics()
    {
        foreach(int code in defaultCosmeticCodes)
        {
            if (!accountData.cosmeticCodes.Contains(code))
            {
                accountData.cosmeticCodes.Add(code);
            }
        }
    }

    public void SetAccountStatus(AccountStatus newStatus)
    {
        accountData.accountStatus = newStatus;
        SaveAccount();
    }

    public void AddItemsToAccount(List<Item> items)
    {
        foreach(Item item in items)
        {
           
                accountData.cosmeticCodes.Add(item.itemCode);
            
            
        }
        
        SaveAccount();

    }
}