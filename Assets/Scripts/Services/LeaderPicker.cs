using UnityEngine;

public class LeaderPicker
{
    public static string PickRandomLeader()
    {
        LocalLobby localLobby = OnlineController.Instance.LocalLobby;

        string id = "";

        int index = 0;

        do
        {
            index = Random.Range(0, localLobby.LocalPlayers.Count);
        } while (localLobby.LocalPlayers[index].PlayerId.Value == localLobby.LeaderID.Value);

        id = localLobby.LocalPlayers[index].PlayerId.Value;

        return id;
    }
}
