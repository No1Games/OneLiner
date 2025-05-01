using UnityEngine;
using System.Threading.Tasks;

public class PlayerPrefsDataStorage : IDataStorage
{
    private const string SaveKey = "PlayerData";

    public Task<bool> HasSaveDataAsync()
    {
        bool hasData = PlayerPrefs.HasKey(SaveKey);
        return Task.FromResult(hasData);
    }

    public Task SaveDataAsync(AccountData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, jsonData);
        PlayerPrefs.Save();
        return Task.CompletedTask;
    }

    public Task<AccountData> LoadDataAsync()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string jsonData = PlayerPrefs.GetString(SaveKey);
            var data = JsonUtility.FromJson<AccountData>(jsonData);
            return Task.FromResult(data);
        }

        return Task.FromResult<AccountData>(null);
    }
}
