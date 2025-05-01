using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GooglePlayGames.BasicApi;

public class GooglePlayDataStorage : IDataStorage
{
    private const string SaveFileName = "PlayerSaveData";

    public async Task<bool> HasSaveDataAsync()
    {
        var metadata = await OpenSaveFileAsync();
        return metadata != null && metadata.TotalTimePlayed > TimeSpan.Zero;
    }

    public async Task SaveDataAsync(AccountData data)
    {
        var metadata = await OpenSaveFileAsync();
        if (metadata == null) return;

        byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        var update = new SavedGameMetadataUpdate.Builder()
            .WithUpdatedDescription("Updated at " + DateTime.Now)
            .Build();

        var tcs = new TaskCompletionSource<bool>();
        PlayGamesPlatform.Instance.SavedGame.CommitUpdate(metadata, update, bytes, (status, game) =>
        {
            if (status == SavedGameRequestStatus.Success)
                tcs.SetResult(true);
            else
            {
                Debug.LogError("Failed to save game data to cloud");
                tcs.SetResult(false);
            }
        });

        await tcs.Task;
    }

    public async Task<AccountData> LoadDataAsync()
    {
        var metadata = await OpenSaveFileAsync();
        if (metadata == null) return null;

        var tcs = new TaskCompletionSource<AccountData>();
        PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(metadata, (status, data) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                string json = Encoding.UTF8.GetString(data);
                var accountData = JsonUtility.FromJson<AccountData>(json);
                tcs.SetResult(accountData);
            }
            else
            {
                Debug.LogError("Failed to load data from cloud");
                tcs.SetResult(null);
            }
        });

        return await tcs.Task;
    }

    private Task<ISavedGameMetadata> OpenSaveFileAsync()
    {
        var tcs = new TaskCompletionSource<ISavedGameMetadata>();
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            SaveFileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                    tcs.SetResult(game);
                else
                {
                    Debug.LogError("Failed to open save file");
                    tcs.SetResult(null);
                }
            });
        return tcs.Task;
    }
}
