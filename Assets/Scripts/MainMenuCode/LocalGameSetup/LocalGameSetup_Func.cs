using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameSetup_Func
{
    #region Setup
    
    public void AddRandomPlayer(List<PlayerScript> players)
    {
        string newPlayerName = "Player " + players.Count;
        int newPlayerID = Random.Range(1, 1000);
        PlayerScript newPlayer = new PlayerScript(newPlayerName, newPlayerID);
        AvatarManager.Instance.SetRandomPlayerSettings(newPlayer);


    }
    public void AddSelectedPlayer(List<PlayerScript> players, PlayerScript player)
    {

    }
    
    
    
    
    //public void IncreasePlayers()
    //{
    //    if (playerAmount < maxPlayers)
    //    {
            
    //        PlayerScript newPlayer;
    //        if (players.Count > 0)
    //        {
                

    //        }
    //        else
    //        {
    //            newPlayer = accountManager.player;
    //        }
    //        players.Add(newPlayer);

    //        GameObject newPlate = Instantiate(playerPlatePrefab, playerPanel.transform);
    //        playersPlates.Add(newPlate);
    //        newPlate.GetComponent<PlateCustomization>().SetPlayer(newPlayer);
    //        newPlate.GetComponentInChildren<Button>().onClick.AddListener(() => PlayerPlateClick(newPlayer));
    //        newPlate.GetComponent<PlateCustomization>().CustomizePlate(avatarManager, newPlayer);


    //        playerAmount++;
    //    }

    //}
    //public void DecreasePlayers()
    //{
    //    if (playerAmount > 0)
    //    {
    //        playerAmount--;
    //        players.RemoveAt(playerAmount);
    //        Destroy(playersPlates[playerAmount]);
    //        playersPlates.RemoveAt(playerAmount);


    //    }

    //}
    //public void NextLevel()
    //{
    //    // Очищення попереднього списку гравців
    //    IngameData.Instance.players.Clear();

    //    if (playerAmount >= minPlayers)
    //    {
    //        foreach (PlayerScript player in players)
    //        {
    //            IngameData.Instance.players.Add(player);
    //        }
    //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Перехід на наступну сцену заблоковано.");
    //    }
    //}

    //public void AssignLeader(PlayerScript newLeader = null)
    //{
    //    if (newLeader == null)
    //    {
    //        int randomLeaderIndex = UnityEngine.Random.Range(0, players.Count);
    //        newLeader = players[randomLeaderIndex];

    //    }

    //    newLeader.role = PlayerRole.Leader;


    //    foreach (PlayerScript p in players)
    //    {
    //        if (p != newLeader)
    //        {
    //            p.role = PlayerRole.Player;
    //            playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(false);
    //        }
    //        else
    //        {
    //            playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(true);
    //        }
    //    }
    //}

    //void PlayerPlateClick(PlayerScript player)
    //{
    //    Debug.Log("plate is clicked");
    //    if (leaderChoosingProcess)
    //    {
    //        AssignLeader(player);
    //        leaderChoosingProcess = false;
    //        Debug.Log("we chose the leader");
    //    }
    //    else
    //    {
    //        Debug.Log("customization in process");
    //        OpenCustomizationFromPlate(player);
    //    }
    //}

    //public void ChoseLeader()
    //{
    //    leaderChoosingProcess = true;
    //    Debug.Log("Choosing started");
    //    // change player plate view as active
    //}

    //public void ChoseRandomLeader()
    //{
    //    AssignLeader();
    //}

    #endregion

}
