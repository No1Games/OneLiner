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

    #region User Guess

    public void OnUserGuess(int index)
    {
        UserGuessServerRpc(index);
    }

    [Rpc(SendTo.Server)]
    private void UserGuessServerRpc(int index)
    {
        RpcEvent guessEvent = new RpcEvent(RpcEventType.UserGuess, index);
        OnRpcEvent?.Invoke(guessEvent);
    }

    #endregion

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
        WrongGuessClientRpc(index, newHearts);
    }

    [ClientRpc]
    private void WrongGuessClientRpc(int index, int newHearts)
    {
        RpcEvent wrongGuessEvent = new RpcEvent(RpcEventType.WrongGuess, (index, newHearts));

        OnRpcEvent?.Invoke(wrongGuessEvent);
    }

    [Rpc(SendTo.Server)]
    private void WrongGuessServerRpc(int index, int newHearts)
    {
        WrongGuessOnServer(index, newHearts);
    }

    #endregion

    #region Game Over

    public void OnGameOver(bool isWin, float score = 0f)
    {
        ExecuteRpc(
            () => GameOverOnServer(isWin, score),
            () => GameOverServerRpc(isWin, score)
        );
    }

    private void GameOverOnServer(bool isWin, float score = 0f)
    {
        RpcEvent gameOverEvent = new RpcEvent(RpcEventType.GameOver, (isWin, score));
        OnRpcEvent?.Invoke(gameOverEvent);

        GameOverClientRpc(isWin, score);
    }

    [Rpc(SendTo.NotServer)]
    private void GameOverClientRpc(bool isWin, float score = 0f)
    {
        RpcEvent gameOverEvent = new RpcEvent(RpcEventType.GameOver, (isWin, score));
        OnRpcEvent?.Invoke(gameOverEvent);
    }

    [Rpc(SendTo.Server)]
    private void GameOverServerRpc(bool isWin, float score = 0f)
    {
        GameOverOnServer(isWin, score);
    }

    #endregion

    #region Spawn Line

    /// <summary>
    /// Spawns a line between the specified start and end points.
    /// </summary>
    /// <param name="start">The starting point of the line in world coordinates.</param>
    /// <param name="end">The ending point of the line in world coordinates.</param>
    public void OnSpawnLine(Vector3 start, Vector3 end)
    {
        ExecuteRpc(
            () => SpawnLineClientRpc(start, end),
            () => SpawnLineServerRpc(start, end)
        );
    }

    /// <summary>
    /// Spawns a line on the server and synchronizes it with connected clients.
    /// </summary>
    /// <param name="start">The starting position of the line in world coordinates.</param>
    /// <param name="end">The ending position of the line in world coordinates.</param>
    private void SpawnLineOnServer(Vector3 start, Vector3 end)
    {
        SpawnLineClientRpc(start, end);
    }

    /// <summary>
    /// Notifies clients and the host to spawn a line between the specified start and end points.
    /// </summary>
    /// <remarks>This method is invoked as a remote procedure call (RPC) and is sent to all clients and the
    /// host. It triggers an event to handle the spawning of the line on the receiving side.</remarks>
    /// <param name="start">The starting point of the line in world coordinates.</param>
    /// <param name="end">The ending point of the line in world coordinates.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnLineClientRpc(Vector3 start, Vector3 end)
    {
        RpcEvent spawnLineEvent = new RpcEvent(RpcEventType.LineSpawned, (start, end));
        OnRpcEvent?.Invoke(spawnLineEvent);
    }

    /// <summary>
    /// Sends a request to the server to spawn a line between two points.
    /// </summary>
    /// <remarks>This method is an RPC (Remote Procedure Call) intended to be invoked by a client to request
    /// the server to spawn a line. The server processes the request and performs the necessary actions.</remarks>
    /// <param name="start">The starting point of the line in world coordinates.</param>
    /// <param name="end">The ending point of the line in world coordinates.</param>
    [Rpc(SendTo.Server)]
    private void SpawnLineServerRpc(Vector3 start, Vector3 end)
    {
        SpawnLineOnServer(start, end);
    }

    #endregion
}
