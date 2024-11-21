
using UnityEngine;

public class PlayerPrefsDataStorage : IDataStorage
{
    private const string SaveKey = "PlayerData";

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public void SaveData(AccountData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, jsonData);
        PlayerPrefs.Save();
    }

    public AccountData LoadData()
    {
        if (HasSaveData())
        {
            string jsonData = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<AccountData>(jsonData);
        }
        return null;
    }
}
