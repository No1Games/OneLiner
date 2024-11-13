using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private NGOLine _linePrefab;
    [SerializeField] private NGODrawManager _drawManager;

    [SerializeField] private OnlineGameSetup _gameSetupManager;

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

    [ClientRpc]
    private void SpawnLineClientRpc(ulong lineNetworkId, Vector3 start, Vector3 end)
    {
        // Get the line instance by NetworkObjectId
        NGOLine line = NetworkManager.SpawnManager.SpawnedObjects[lineNetworkId].GetComponent<NGOLine>();

        if (line != null)
        {
            line.SetLinePositions(start, end);

            _drawManager.AddLine(line);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnLineServerRpc(Vector3 start, Vector3 end)
    {
        SpawnLineOnServer(start, end);
    }

    private void SpawnLineOnServer(Vector3 start, Vector3 end)
    {
        // Instantiate the line on the server and set positions
        NGOLine line = Instantiate(_linePrefab);
        line.SetLinePositions(start, end);

        _drawManager.AddLine(line);

        // Spawn it across the network
        line.NetworkObject.Spawn();

        // Synchronize initial positions across all clients
        SpawnLineClientRpc(line.NetworkObjectId, start, end);
    }

    #endregion

    #region Wrong Guess Syncronization

    public void OnGuessedWrong(int index, int count)
    {
        if (IsHost)
        {
            OnGuessedWrongOnHost(index, count);

            OnGuessedWrongClientRpc(index, count);
        }
        else
        {
            OnGuessedWrongServerRpc(index, count);
        }
    }

    private void OnGuessedWrongOnHost(int index, int count)
    {
        DisableButton(index);

        UpdateHearts(count);

        OnGuessedWrongClientRpc(index, count);
    }

    [ClientRpc]
    private void OnGuessedWrongClientRpc(int index, int count)
    {
        DisableButton(index);

        UpdateHearts(count);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnGuessedWrongServerRpc(int index, int count)
    {
        OnGuessedWrongOnHost(index, count);
    }

    private void DisableButton(int index)
    {
        _gameSetupManager.DisableButtonByIndex(index);
    }

    private void UpdateHearts(int count)
    {
        _gameSetupManager.SetHearts(count);
    }

    #endregion

    #region Game Over Syncronization

    private void ShowGameOverScreen(bool isWin, float score = 0f)
    {
        _gameSetupManager.ShowGameOverScreen(isWin, score);
    }

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
        ShowGameOverScreen(isWin, score);

        GameOverClientRpc(isWin, score);
    }

    [ClientRpc]
    private void GameOverClientRpc(bool isWin, float score = 0f)
    {
        ShowGameOverScreen(isWin, score);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GameOverServerRpc(bool isWin, float score = 0f)
    {
        GameOverOnHost(isWin, score);
    }

    #endregion
}
