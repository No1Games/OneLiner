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


    public bool isTutorialOn = false;  // temporarily


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
    }

    public async Task InitializeAccountAsync(bool isAuthenticated)
    {
        Debug.Log("Initializing account started");
        // Обираємо джерело даних
        IDataStorage storage = isAuthenticated
            ? new GooglePlayDataStorage()
            : new PlayerPrefsDataStorage();

        saveManager = new SaveManager(storage);
        Debug.Log("Storage initialized? start checking save file");
        // Перевіряємо, чи є сейви
        bool hasSave = await saveManager.HasSaveAsync();
        Debug.Log("Save file checked it is" + hasSave);
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

        InitializePlayer(CurrentAccountData);
        Debug.Log("player initialized");
        UpdateDefaultCosmetics();
        Debug.Log("cosmetic updated");
        await Task.Delay(3000);
        OnAccountInitializationComplete?.Invoke();
        Debug.Log("Initializing account complited");

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