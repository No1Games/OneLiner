using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    private Action _onConnectionVerified;
    private int _expectedPlayerCount;
    private Action onGameBeginning;
    private Action _onGameEnd;
    private bool? _canSpawnIngameObjects;
    private PlayerScript _localUserData;

    [SerializeField] private NGOLine _linePrefab;
    [SerializeField] private NGODrawManager _drawManager;

    public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
    {
        _onConnectionVerified = onConnectionVerified;
        _expectedPlayerCount = expectedPlayerCount;
        onGameBeginning = onGameBegin;
        _onGameEnd = onGameEnd;
        _canSpawnIngameObjects = null;
        _localUserData = new PlayerScript(localUser.DisplayName.Value, 0);
    }

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
}
