using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayDataStorage : IDataStorage
{
    public bool HasSaveData()
    {
        // ����� �������� �������� ���������� ����� � Google Play Saved Games
        return false;
    }

    public void SaveData(AccountData data)
    {
        // ����� ���������� ����� � Google Play Saved Games
    }

    public AccountData LoadData()
    {
        // ����� ������������ ��������� � Google Play Saved Games
        return null;
    }
}
