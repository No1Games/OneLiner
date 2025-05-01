using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class SaveManager
{
    private readonly IDataStorage dataStorage;
    private readonly Queue<AccountData> saveQueue = new();
    private readonly object queueLock = new();
    private bool isProcessing = false;
    private readonly TimeSpan saveInterval = TimeSpan.FromSeconds(5);

    public SaveManager(IDataStorage storage)
    {
        dataStorage = storage;
        StartSaveLoop();
    }

    public void EnqueueSave(AccountData data)
    {
        lock (queueLock)
        {
            saveQueue.Enqueue(data);
        }
    }

    private void StartSaveLoop()
    {
        if (!isProcessing)
        {
            isProcessing = true;
            _ = ProcessSaveQueueAsync();
        }
    }

    private async Task ProcessSaveQueueAsync()
    {
        while (isProcessing)
        {
            await Task.Delay(saveInterval);

            AccountData dataToSave = null;

            lock (queueLock)
            {
                if (saveQueue.Count > 0)
                {
                    // Забираємо останню актуальну версію (можна ще перед цим робити DistinctBy або порівняння, якщо потрібно)
                    while (saveQueue.Count > 1) saveQueue.Dequeue();
                    dataToSave = saveQueue.Dequeue();
                }
            }

            if (dataToSave != null)
            {
                try
                {
                    await dataStorage.SaveDataAsync(dataToSave);
                    UnityEngine.Debug.Log("AccountData saved via queue");
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"Save failed: {e.Message}");
                    // Можна повторно закинути в чергу, якщо потрібно
                }
            }
        }
    }

    public Task<AccountData> LoadAsync() => dataStorage.LoadDataAsync();
    public Task<bool> HasSaveAsync() => dataStorage.HasSaveDataAsync();

   
}