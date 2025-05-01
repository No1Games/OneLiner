using System.Threading.Tasks;

public interface IDataStorage
{
    Task<bool> HasSaveDataAsync();
    Task SaveDataAsync(AccountData data);
    Task<AccountData> LoadDataAsync();
}