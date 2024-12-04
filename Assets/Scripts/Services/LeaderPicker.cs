using UnityEngine;

public class LeaderPicker
{
    public static string PickRandomLeader()
    {
        LocalLobby localLobby = OnlineGameManager.Instance.LocalLobby;

        string id = "";

        int index = 0;

        do
        {
            index = Random.Range(0, localLobby.LocalPlayers.Count);
        } while (localLobby.LocalPlayers[index].ID.Value == localLobby.LeaderID.Value);

        id = localLobby.LocalPlayers[index].ID.Value;

        return id;
    }
}
