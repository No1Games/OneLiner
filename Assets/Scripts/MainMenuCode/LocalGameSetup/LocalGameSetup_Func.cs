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

    public void AddRandomPlayer(List <PlayerScript> players)
    {
        
            string newPlayerName = "Player " + players.Count;
            int newPlayerID = Random.Range(1, 1000);
            PlayerScript newPlayer = new PlayerScript(newPlayerName, newPlayerID);
            AvatarManager.Instance.SetRandomPlayerSettings(newPlayer);
            players.Add(newPlayer);                    
    }

    public GameObject GeneratePlayerPlate(PlayerScript player)
    {
        GameObject newPlate = Instantiate(platePrefab, playerPanel.transform);
        newPlate.GetComponent<PlateCustomization>().CustomizePlate(AvatarManager.Instance, player);
        newPlate.GetComponent<PlateCustomization>().SetPlayer(player);
        return newPlate;

        
        
        //newPlate.GetComponentInChildren<Button>().onClick.AddListener(() => PlayerPlateClick(newPlayer));
        //newPlate.GetComponent<PlateCustomization>().CustomizePlate(avatarManager, newPlayer);
//AssignLeader(newPlayer);




    }

    }
//    public void DecreasePlayers()
//    {
//        if (playerAmount > 0)
//        {
//            playerAmount--;
//            players.RemoveAt(playerAmount);
//            Destroy(playersPlates[playerAmount]);
//            playersPlates.RemoveAt(playerAmount);


//        }

//    }
//    public void NextLevel()
//    {
//        // Очищення попереднього списку гравців
//        IngameData.Instance.players.Clear();

//        if (playerAmount >= minPlayers)
//        {
//            foreach (PlayerScript player in players)
//            {
//                IngameData.Instance.players.Add(player);
//            }
//            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Play);
//            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
//        }
//        else
//        {
//            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
//            Debug.LogWarning("Перехід на наступну сцену заблоковано.");
//        }
//    }

//    public void AssignLeader(PlayerScript newLeader = null)
//    {
//        if (newLeader == null)
//        {
//            int randomLeaderIndex = UnityEngine.Random.Range(0, players.Count);
//            newLeader = players[randomLeaderIndex];

//        }

//        newLeader.role = PlayerRole.Leader;


//        foreach (PlayerScript p in players)
//        {
//            if (p != newLeader)
//            {
//                p.role = PlayerRole.Player;
//                playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(false);
//            }
//            else
//            {
//                playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(true);
//            }
//        }
//    }

//    void PlayerPlateClick(PlayerScript player)
//    {

//        if (leaderChoosingProcess)
//        {
//            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
//            AssignLeader(player);
//            leaderChoosingProcess = false;

//        }
//        else
//        {
//            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_edit);
//            OpenCustomizationFromPlate(player);
//        }
//    }

//    public void ChoseLeader()
//    {
//        if (players.Count > 0)
//        {
//            leaderChoosingProcess = true;
//            Debug.Log("Choosing started");
//            // change player plate view as active
//        }

//    }

//    public void ChoseRandomLeader()
//    {
//        if (players.Count > 0)
//        {
//            AssignLeader();
//        }

//    }
//}
