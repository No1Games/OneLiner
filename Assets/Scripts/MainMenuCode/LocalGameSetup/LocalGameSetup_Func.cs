using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameSetup_Func : MonoBehaviour
{
    private GameObject platePrefab;
    private GameObject playerPanel;


    public LocalGameSetup_Func(GameObject platePrefab, GameObject playerPanel)
    {
        this.platePrefab = platePrefab;
        this.playerPanel = playerPanel;

    }

    public PlayerScript AddRandomPlayer(List<PlayerScript> players)
    {

        string newPlayerName = "Player " + players.Count;
        int newPlayerID = Random.Range(1, 1000);
        PlayerScript newPlayer = new PlayerScript(newPlayerName, newPlayerID);
        AvatarManager.Instance.SetRandomPlayerSettings(newPlayer);
        return newPlayer;
    }

    public GameObject GeneratePlayerPlate(PlayerScript player)
    {
        GameObject newPlate = Instantiate(platePrefab, playerPanel.transform);
        newPlate.GetComponent<PlateCustomization>().CustomizePlate(AvatarManager.Instance, player);
        newPlate.GetComponent<PlateCustomization>().SetPlayer(player);
        return newPlate;
    }

    public void NextLevel(List<PlayerScript> players)
    {
        // Очищення попереднього списку гравців
        IngameData.Instance.players.Clear();
        foreach (PlayerScript player in players)
        {
            if(player.role == PlayerRole.NotSetYet)
            {
                player.role = PlayerRole.Player;
            }
            IngameData.Instance.players.Add(player);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void AssignLeader(List<GameObject> playersPlates,  PlayerScript newLeader = null)
    {

        if (newLeader == null)
        {
            do
            {
                int randomLeaderIndex = UnityEngine.Random.Range(0, playersPlates.Count);
                newLeader = playersPlates[randomLeaderIndex].GetComponent<PlateCustomization>().player;

            }
            while (newLeader.role == PlayerRole.Leader); 
        }

        newLeader.role = PlayerRole.Leader;

        foreach(GameObject plate in playersPlates)
        {
            PlateCustomization plateInfo = plate.GetComponent<PlateCustomization>();
            if (plateInfo.player == newLeader)
            {
                plateInfo.leaderCrown.SetActive(true);
            }
            else
            {
                plateInfo.player.role = PlayerRole.Player;
                plateInfo.leaderCrown.SetActive(false);

            }
        }

    }


}




