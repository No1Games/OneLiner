using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerScript> players;
    public List<PlayerScript> turnsQueue;

    private int actualTurnPosition = 0;

    public event Action<PlayerScript> OnPlayerChange;

    [Header("Plates")]
    [SerializeField] GameObject platePrefab;
    [SerializeField] GameObject playersPanel;    
    List<GameObject> playersPlates = new();


    private void Start()
    {
        players = new List<PlayerScript>(IngameData.Instance.players);

        GeneratePlates();

        GenerateTurnQueue();

        OnPlayerChange?.Invoke(turnsQueue[actualTurnPosition]);


    }

    void AssignRoles()
    {

        int randomLeaderIndex = UnityEngine.Random.Range(0, players.Count); // Випадковий індекс для ведучого


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

    public void ChangeTurn()
    {

        if (actualTurnPosition + 1 < turnsQueue.Count)
        {
            actualTurnPosition++;
        }
        else
        {
            actualTurnPosition = 0;
        }
        OnPlayerChange.Invoke(turnsQueue[actualTurnPosition]);

    }

    void GeneratePlates()
    {
        foreach (PlayerScript player in players)
        {
            GameObject playerPlate = Instantiate(platePrefab, playersPanel.transform);
            PlateCustomization customization = playerPlate.GetComponent<PlateCustomization>();
            customization.CustomizePlate(player);

            if (player.role == PlayerRole.Leader)
            {
             customization.leaderCrown.SetActive(true);
            }


        }
    }


}
