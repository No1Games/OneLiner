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
    [SerializeField] private TMP_Text playerNameOnTop;
    [SerializeField] private TMP_Text playerName;


    private int currentPlayer = 0;


    private void Awake()
    {

        drawing.OnDrawingComplete += CallCheckUpMenu;
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
    //не факт що треба
    void QueueStepping()
    {
        int playersTurn = 0;
        PlayerScript currentPlayer = players.Find(p => p.playerID == playersTurn);
    }

    void OpenPlayerScreen()
    {
        playerNameOnTop.text = turnsQueue[currentPlayer].name;
        playerName.text = turnsQueue[currentPlayer].name;

        startPanel.SetActive(true);
        endPanel.SetActive(false);

    }

    public void StartTurn ()
    {
        startPanel.SetActive(false);
        drawing.DrawingAllowed = true;
    }

    void CallCheckUpMenu()
    {
        endPanel.SetActive(true);
        drawing.DrawingAllowed = false;
    }
    
    public void RestartTurn()
    {
        endPanel.SetActive(false);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void EndTurn()
    {
        
        if (currentPlayer+1 < turnsQueue.Count)
        {
            currentPlayer++;
        }
        else
        {
            currentPlayer = 0;
        }
        OpenPlayerScreen();


    }

}
