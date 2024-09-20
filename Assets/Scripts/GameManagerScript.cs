using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManagerScript : MonoBehaviour
{
    private List<PlayerScript> players;
    public List<PlayerScript> turnsQueue;
    [SerializeField] private DrawingScript drawing;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject wordPanel;
    [SerializeField] private GameObject leaderPanel;
    [SerializeField] private GameObject ingamePanel;
    [SerializeField] private TMP_Text playerNameOnTop;
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private WordManager wordManager;


    private int currentPlayerIndex = 0;


    private void Awake()
    {

        drawing.OnDrawingComplete += CallCheckUpMenu;
        wordManager.gameEnded += OpenEndGameMenu;
        AssignRoles();
        GenerateTurnQueue();
        OpenPlayerScreen();

    }
    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Debug.Log(IngameData.Instance.players.Count);
        }
    }

    

    void AssignRoles()
    {
        players = new List<PlayerScript>(IngameData.Instance.players);
        int randomLeaderIndex = Random.Range(0, players.Count); // Випадковий індекс для ведучого

        
        players[randomLeaderIndex].role = PlayerRole.Leader;

        
        for (int i = 0; i < players.Count; i++)
        {
            if (i != randomLeaderIndex)
            {
                players[i].role = PlayerRole.Player;
            }
        }
    }

    void GenerateTurnQueue()
    {
        turnsQueue = new List<PlayerScript>(); 

        // Знаходимо ведучого
        PlayerScript leader = players.Find(p => p.role == PlayerRole.Leader);

        
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].role == PlayerRole.Player) 
            {
                turnsQueue.Add(leader);                      
                turnsQueue.Add(players[i]);
            }
        }

        
    }
    

    void OpenPlayerScreen()
    {
        playerNameOnTop.text = turnsQueue[currentPlayerIndex].name;
        playerName.text = turnsQueue[currentPlayerIndex].name;

        startPanel.SetActive(true);
        endPanel.SetActive(false);
        ingamePanel.SetActive(false);

    }

    public void StartTurn ()
    {
        startPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.DrawingAllowed = true;
    }

    void CallCheckUpMenu()
    {
        endPanel.SetActive(true);
        ingamePanel.SetActive(false);
        drawing.DrawingAllowed = false;
    }
    
    public void RestartTurn()
    {
        endPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void EndTurn()
    {
        
        if (currentPlayerIndex+1 < turnsQueue.Count)
        {
            currentPlayerIndex++;
        }
        else
        {
            currentPlayerIndex = 0;
        }
        OpenPlayerScreen();


    }

    public void OpenWordsMenu()
    {
        ingamePanel.SetActive(false);
        drawing.DrawingAllowed = false;
        if (turnsQueue[currentPlayerIndex].role == PlayerRole.Leader)
        {
            leaderPanel.SetActive(true);
        }
        else
        {
            wordPanel.SetActive(true);                   

        }
        
    }

    public void CloseWordsMenu()
    {
        wordPanel.SetActive(false);
        leaderPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.DrawingAllowed = true;
    }

    private void OpenEndGameMenu(bool isWin)
    {
        if (isWin)
        {

        }
        else
        {
            
        }

    }

}
