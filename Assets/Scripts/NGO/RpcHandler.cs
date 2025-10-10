using System;
using Unity.Netcode;
using UnityEngine;

public class RpcHandler : NetworkBehaviour
{
    public event Action<RpcEvent> OnRpcEvent;

    private void ExecuteRpc(Action serverAction, Action clientAction)
    {
        if (IsHost) serverAction?.Invoke();
        else clientAction?.Invoke();
    }

    public void OnUserGuess(int index)
    {
        UserGuessServerRpc(index);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UserGuessServerRpc(int index)
    {
        RpcEvent guessEvent = new RpcEvent(RpcEventType.UserGuess, index);
        OnRpcEvent?.Invoke(guessEvent);
    }

    #region Wrong Guess

    public void OnWrongGuess(int index, int newHearts)
    {
        ExecuteRpc(
            () => WrongGuessOnServer(index, newHearts),
            () => WrongGuessServerRpc(index, newHearts)
        );
    }

    private void WrongGuessOnServer(int index, int newHearts)
    {
        RpcEvent wordDisableEvent = new RpcEvent(RpcEventType.WordDisabled, index);
        RpcEvent heartUpdateEvent = new RpcEvent(RpcEventType.HeartsUpdated, newHearts);

        OnRpcEvent?.Invoke(wordDisableEvent);
        OnRpcEvent?.Invoke(heartUpdateEvent);

        WrongGuessClientRpc(index, newHearts);
    }

    [ClientRpc]
    private void WrongGuessClientRpc(int index, int newHearts)
    {
        if (IsHost) return;

        RpcEvent wordDisableEvent = new RpcEvent(RpcEventType.WordDisabled, index);
        RpcEvent heartUpdateEvent = new RpcEvent(RpcEventType.HeartsUpdated, newHearts);

        OnRpcEvent?.Invoke(wordDisableEvent);
        OnRpcEvent?.Invoke(heartUpdateEvent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WrongGuessServerRpc(int index, int newHearts)
    {
        WrongGuessOnServer(index, newHearts);
    }

    #endregion

    #region Game Over

    public void OnGameOver(bool isWin, float score = 0f)
    {
        ExecuteRpc(
            () => GameOverServerRpc(isWin, score),
            () => GameOverClientRpc(isWin, score)
        );
    }

    private void GameOverOnServer(bool isWin, float score = 0f)
    {
        RpcEvent gameOverEvent = new RpcEvent(RpcEventType.GameOver, (isWin, score));
        OnRpcEvent?.Invoke(gameOverEvent);

        GameOverClientRpc(isWin, score);
    }

    [ClientRpc]
    private void GameOverClientRpc(bool isWin, float score = 0f)
    {
        if (IsHost) return;

        RpcEvent gameOverEvent = new RpcEvent(RpcEventType.GameOver, (isWin, score));
        OnRpcEvent?.Invoke(gameOverEvent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GameOverServerRpc(bool isWin, float score = 0f)
    {
        GameOverOnServer(isWin, score);
    }

    #endregion

    #region Spawn Line

    public void OnSpawnLine(Vector3 start, Vector3 end)
    {
        ExecuteRpc(
            () => SpawnLineServerRpc(start, end),
            () => SpawnLineClientRpc(start, end)
        );
    }

    private void SpawnLineOnServer(Vector3 start, Vector3 end)
    {
        RpcEvent spawnLineEvent = new RpcEvent(RpcEventType.LineSpawned, (start, end));
        OnRpcEvent?.Invoke(spawnLineEvent);

        SpawnLineClientRpc(start, end);
    }

    [ClientRpc]
    private void SpawnLineClientRpc(Vector3 start, Vector3 end)
    {
        if (IsHost) return;

        RpcEvent spawnLineEvent = new RpcEvent(RpcEventType.LineSpawned, (start, end));
        OnRpcEvent?.Invoke(spawnLineEvent);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnLineServerRpc(Vector3 start, Vector3 end)
    {
        SpawnLineOnServer(start, end);
    }

    #endregion

    #region Legacy
    //[SerializeField] private Line _linePrefab;
    //[SerializeField] private NGODrawManager _drawManager;
    //public event Action<int> DisableWordEvent;
    //public event Action<int> UpdateHeartsEvent;
    //public event Action<bool, float> GameOverEvent;
    /* OnGuessedWrong
    public void OnGuessedWrong(int index, int newHeartsCount)
    {
        if (IsHost)
        {
            OnGuessedWrongOnServer(index, newHeartsCount);
        }
        else
        {
            OnGuessedWrongServerRpc(index, newHeartsCount);
        }
    }
    */
    /* OnGuessedWrongOnServer
    private void OnGuessedWrongOnServer(int index, int count)
    {
        DisableWordButtonEvent?.Invoke(index);
        UpdateHeartsEvent?.Invoke(count);

        OnGuessedWrongClientRpc(index, count);
    }
    */
    /* OnGuessedWrongClientRpc
    [ClientRpc]
    private void OnGuessedWrongClientRpc(int index, int count)
    {
        if (IsHost) return;
        DisableWordButtonEvent?.Invoke(index);
        UpdateHeartsEvent?.Invoke(count);
    }
    */
    /* OnGuessedWrongServerRpc
    [ServerRpc(RequireOwnership = false)]
    private void OnGuessedWrongServerRpc(int index, int count)
    {
        OnGuessedWrongOnServer(index, count);
    }
    */
    /* OnGameOver
    public void OnGameOver(bool isWin, float score = 0f)
    {
        if (IsHost)
        {
            GameOverOnHost(isWin, score);
        }
        else
        {
            GameOverServerRpc(isWin, score);
        }
    }
    */
    /* GameOverOnHost
    private void GameOverOnHost(bool isWin, float score = 0f)
    {
        GameOverEvent?.Invoke(isWin, score);

        GameOverClientRpc(isWin, score);
    }
    */
    /* GameOverClientRpc
    [ClientRpc]
    private void GameOverClientRpc(bool isWin, float score = 0f)
    {
        if (IsHost) return;
        GameOverEvent?.Invoke(isWin, score);
    }
    */
    /* GameOverServerRpc
     [ServerRpc(RequireOwnership = false)]
    private void GameOverServerRpc(bool isWin, float score = 0f)
    {
        GameOverOnHost(isWin, score);
    }
     */
    /* SpawnLine
    public void SpawnLine(Vector3 start, Vector3 end)
    {
        if (IsHost)
        {
            SpawnLineOnServer(start, end);
        }
        else if (IsClient)
        {
            SpawnLineServerRpc(start, end);
        }
    }
    */
    /* SpawnLineOnServer
    private void SpawnLineOnServer(Vector3 start, Vector3 end)
    {
        // Instantiate the line on the server and set positions
        Line line = _drawManager.SpawnLine(start, end);

        // Synchronize initial positions across all clients
        SpawnLineClientRpc(start, end);
    }
    */
    /* SpawnLineClientRpc
    [ClientRpc]
    private void SpawnLineClientRpc(Vector3 start, Vector3 end)
    {
        if (IsHost) return;
        Line line = _drawManager.SpawnLine(start, end);
    }
    */
    /* SpawnLineServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void SpawnLineServerRpc(Vector3 start, Vector3 end)
    {
        SpawnLineOnServer(start, end);
    }
    */
    #endregion
}
