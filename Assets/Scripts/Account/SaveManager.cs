

public class SaveManager
{
    private IDataStorage dataStorage;

    public SaveManager(IDataStorage storage)
    {
        dataStorage = storage;
    }

    public void Save(AccountData data)
    {
        dataStorage.SaveData(data);
    }

    public AccountData Load()
    {
        return dataStorage.LoadData();
    }

    public bool HasSave()
    {
        return dataStorage.HasSaveData();
    }
}

