public interface IDataStorage
{
    bool HasSaveData();
    void SaveData(AccountData data);
    AccountData LoadData();
}