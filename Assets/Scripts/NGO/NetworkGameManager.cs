using System;
using Unity.Netcode;

public class NetworkGameManager : NetworkBehaviour
{
    private Action _onConnectionVerified;
    private int _expectedPlayerCount;
    private Action onGameBeginning;
    private Action _onGameEnd;
    private bool? _canSpawnIngameObjects;
    private PlayerScript _localUserData;

    public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
    {
        _onConnectionVerified = onConnectionVerified;
        _expectedPlayerCount = expectedPlayerCount;
        onGameBeginning = onGameBegin;
        _onGameEnd = onGameEnd;
        _canSpawnIngameObjects = null;
        _localUserData = new PlayerScript(localUser.DisplayName.Value, 0);
    }
}
