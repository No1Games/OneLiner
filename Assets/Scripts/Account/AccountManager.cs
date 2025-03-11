using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GooglePlayGames;


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




    public void PrepareAccountData(bool result)
    {
        InitializeStorage(result);



        if (saveManager.HasSave())
        {
            accountData = saveManager.Load();
            InitializeAccount(accountData);
        }
        else
        {
            accountData = CreateDefaultAccount(result);
            SaveAccount();


        }
        UpdateDefaultCosmetics();

        // call main screen customisation
        // close loading screen
        //if account isnt auth in google show information about limitation
    }
    private void InitializeStorage(bool goFromGoogle)
    {


        if (goFromGoogle)
        {
            dataStorage = new GooglePlayDataStorage();
        }
        else
        {
            dataStorage = new PlayerPrefsDataStorage();

        }
        saveManager = new SaveManager(dataStorage);
    }


    private void InitializeAccount(AccountData data)
    {
        // Ініціалізація акаунта (можливо, присвоєння значень у грі)
        player = new PlayerScript(data.playerName);
        player.UpdatePlayerInfo(data.playerName, data.avatarCode, data.avatarBackgroundCode, data.nameBackgroundCode);
    }

    private AccountData CreateDefaultAccount(bool hasDataFromGoogle)
    {
        AccountData _newAccount = new AccountData
        {
            playerName = "Guest",
            playerID = Guid.NewGuid().ToString(),
            avatarCode = 1001,
            avatarBackgroundCode = 2001,
            nameBackgroundCode = 3001,
            cosmeticCodes = defaultCosmeticCodes,
            gems = 0,
            experience = 0,
            level = 1,
            accountStatus = AccountStatus.Offline,
            completeTutorial = false,

        };

        if (hasDataFromGoogle)
        {
            _newAccount.playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
            _newAccount.playerID = PlayGamesPlatform.Instance.GetUserId();
            _newAccount.accountStatus = AccountStatus.Basic;
        }
        return _newAccount;
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
        foreach (int code in defaultCosmeticCodes)
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
        foreach (Item item in items)
        {

            accountData.cosmeticCodes.Add(item.itemCode);


        }

        SaveAccount();

    }
}