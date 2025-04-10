using System;
using Unity.Netcode;
using UnityEngine;

public class RpcHandler : NetworkBehaviour
{
    [SerializeField] private Line _linePrefab;
    [SerializeField] private NGODrawManager _drawManager;

    //[SerializeField] private OnlineGameSetup _gameSetupManager;

    public event Action<int> DisableWordButtonEvent;
    public event Action<int> UpdateHeartsEvent;
    public event Action<bool, float> GameOverEvent;

    #region Line Syncronization

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

    private void SpawnLineOnServer(Vector3 start, Vector3 end)
    {
        // Instantiate the line on the server and set positions
        Line line = _drawManager.SpawnLine(start, end);

        // Synchronize initial positions across all clients
        SpawnLineClientRpc(start, end);
    }

    [ClientRpc]
    private void SpawnLineClientRpc(Vector3 start, Vector3 end)
    {
        if (IsHost) return;
        Line line = _drawManager.SpawnLine(start, end);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnLineServerRpc(Vector3 start, Vector3 end)
    {
        SpawnLineOnServer(start, end);
    }


    #endregion

    #region Wrong Guess Syncronization

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

    private void OnGuessedWrongOnServer(int index, int count)
    {
        DisableWordButtonEvent?.Invoke(index);
        UpdateHeartsEvent?.Invoke(count);

        OnGuessedWrongClientRpc(index, count);
    }

    [ClientRpc]
    private void OnGuessedWrongClientRpc(int index, int count)
    {
        DisableWordButtonEvent?.Invoke(index);
        UpdateHeartsEvent?.Invoke(count);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnGuessedWrongServerRpc(int index, int count)
    {
        OnGuessedWrongOnServer(index, count);
    }

    #endregion

    #region Game Over Syncronization

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

    private void GameOverOnHost(bool isWin, float score = 0f)
    {
        GameOverEvent?.Invoke(isWin, score);

        GameOverClientRpc(isWin, score);
    }

    [ClientRpc]
    private void GameOverClientRpc(bool isWin, float score = 0f)
    {
        GameOverEvent?.Invoke(isWin, score);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GameOverServerRpc(bool isWin, float score = 0f)
    {
        GameOverOnHost(isWin, score);
    }

    #endregion
}
