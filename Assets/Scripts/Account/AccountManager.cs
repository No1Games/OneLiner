using System.Threading.Tasks;
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

    private SaveManager saveManager;
    public AccountData CurrentAccountData { get; private set; }
    public PlayerScript player;
    private SubscriptionManager subscriptionManager;

    


    [Header("Default Items")]
    [SerializeField] private List<int> defaultCosmeticCodes;

    public event Action OnAccountInitializationComplete;

    public static AccountManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        subscriptionManager = new();
    }

    public async Task InitializeAccountAsync(bool isAuthenticated)
    {
        
        // Обираємо джерело даних
        IDataStorage storage = isAuthenticated
            ? new GooglePlayDataStorage()
            : new PlayerPrefsDataStorage();

        saveManager = new SaveManager(storage);
        
        // Перевіряємо, чи є сейви
        bool hasSave = await saveManager.HasSaveAsync();
       
        if (hasSave)
        {
            CurrentAccountData = await saveManager.LoadAsync();
            Debug.Log("Account loaded");
        }
        else
        {
            CurrentAccountData = CreateDefaultAccount(isAuthenticated);
            SaveAccountData();
            Debug.Log("New account created and saved");
        }
        if (isAuthenticated)
        {
            await subscriptionManager.CheckSubscription(CurrentAccountData);
        }

        InitializePlayer(CurrentAccountData);
        
        UpdateDefaultCosmetics();
        
        await Task.Delay(3000);
        OnAccountInitializationComplete?.Invoke();
        

    }


    private void InitializePlayer(AccountData data)
    {

        player = new PlayerScript(data.playerName);
        player.UpdatePlayerInfo(data.playerName, data.avatarCode, data.avatarBackgroundCode, data.nameBackgroundCode);
    }

    private AccountData CreateDefaultAccount(bool hasDataFromGoogle)
    {
        AccountData _newAccount = new AccountData
        {
            playerName = "NewFriend",
            playerID = Guid.NewGuid().ToString(),
            avatarCode = 1001,
            avatarBackgroundCode = 2001,
            nameBackgroundCode = 3001,
            cosmeticCodes = defaultCosmeticCodes,
            gems = 0,
            experience = 0,
            level = 1,
            accountStatus = AccountStatus.Offline,
            tutorialStatus = TutorialStatus.None,       


};

        if (hasDataFromGoogle)
        {
            _newAccount.playerName = PlayGamesPlatform.Instance.GetUserDisplayName();
            _newAccount.playerID = PlayGamesPlatform.Instance.GetUserId();
            _newAccount.accountStatus = AccountStatus.Basic;
        }
        return _newAccount;
    }





    private void UpdateDefaultCosmetics()
    {
        foreach (int code in defaultCosmeticCodes)
        {
            if (!CurrentAccountData.cosmeticCodes.Contains(code))
            {
                CurrentAccountData.cosmeticCodes.Add(code);
            }
        }
    }

    public void SetAccountStatus(AccountStatus newStatus)
    {
        CurrentAccountData.accountStatus = newStatus;
        SaveAccountData();
    }

    public void AddItemsToAccount(List<Item> items)
    {
        foreach (Item item in items)
        {

            CurrentAccountData.cosmeticCodes.Add(item.itemCode);


        }

        SaveAccountData();

    }

    public void SaveAccountData()
    {
        saveManager.EnqueueSave(new AccountData(CurrentAccountData));
    }
}