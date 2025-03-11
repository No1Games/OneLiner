using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System;
using UnityEngine;
using System.Threading.Tasks;

public class GooglePlayDataStorage : IDataStorage
{
    private const string SaveFileName = "PlayerSaveData";

    public bool HasSaveData()
    {
        var task = CheckSaveDataAsync();
        task.Wait(); // чекаємо результат асинхронної операції
        return task.Result; // повертаємо результат
    }

    private async Task<bool> CheckSaveDataAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        OpenSaveFile((status, game) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                tcs.SetResult(game.TotalTimePlayed > TimeSpan.Zero);
            }
            else
            {
                Debug.LogError("Failed to open save file to check existence");
                tcs.SetResult(false);
            }
        });

        return await tcs.Task; // повертаємо асинхронно отримане значення
    }

    public void SaveData(AccountData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        OpenSaveFile((status, game) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                byte[] saveData = System.Text.Encoding.UTF8.GetBytes(jsonData);
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                    .WithUpdatedDescription("Player data updated")
                    .Build();

                PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, saveData, OnSaveComplete);
            }
            else
            {
                Debug.LogError("Failed to open save file for saving");
            }
        });
    }

    public AccountData LoadData()
    {
        var task = LoadDataAsync();
        task.Wait(); // чекаємо результат асинхронної операції
        return task.Result; // повертаємо результат
    }

    private async Task<AccountData> LoadDataAsync()
    {
        var tcs = new TaskCompletionSource<AccountData>();

        OpenSaveFile((status, game) =>
        {
            if (status == SavedGameRequestStatus.Success)
            {
                PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (readStatus, data) =>
                {
                    if (readStatus == SavedGameRequestStatus.Success)
                    {
                        string jsonData = System.Text.Encoding.UTF8.GetString(data);
                        var accountData = JsonUtility.FromJson<AccountData>(jsonData);
                        tcs.SetResult(accountData);
                    }
                    else
                    {
                        Debug.LogError("Failed to read save data");
                        tcs.SetResult(null);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to open save file for loading");
                tcs.SetResult(null);
            }
        });

        return await tcs.Task; // повертаємо асинхронно отримане значення
    }

    private void OpenSaveFile(Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
    {
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            SaveFileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            callback
        );
    }

    private void OnSaveComplete(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("Game data successfully saved to Google Play");
        }
        else
        {
            Debug.LogError("Failed to save game data");
        }
    }
}
