using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSetupMenu : MonoBehaviour
{
    private int playerAmount = 0;
    private int maxPlayers = 7;
    private int minPlayers = 0;
    [SerializeField] private TMP_Text playerNumber;

    [SerializeField] private List<GameObject> playersFields;

    public void IncreasePlayers()
    {
        if (playerAmount < maxPlayers && playersFields.Count  > 0)
        {
            playersFields[playerAmount].SetActive(true);
            playerAmount++;
        }
        
    }
    public void DecreasePlayers() 
    { 
        if(playerAmount > minPlayers && playersFields.Count > 0)
        {
            playerAmount--;
            playersFields[playerAmount].SetActive(false);
        }
    
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void Update()
    {
        playerNumber.text = playerAmount.ToString();
    }
}
