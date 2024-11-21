using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayDataStorage : IDataStorage
{
    public bool HasSaveData()
    {
        // Логіка перевірки наявності збережених даних в Google Play Saved Games
        return false;
    }

    public void SaveData(AccountData data)
    {
        // Логіка збереження даних у Google Play Saved Games
    }

    public AccountData LoadData()
    {
        // Логіка завантаження збережень з Google Play Saved Games
        return null;
    }
}
