using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerScript> players;
    private List<PlayerScript> turnsQueue;

    private int actualTurnPosition = 0;

    public event Action<PlayerScript> OnPlayerChange;

    [Header("Plates")]
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private GameObject playersPanel;

    private Dictionary<PlayerScript, GameObject> playerPlates = new Dictionary<PlayerScript, GameObject>();

    private void Start()
    {
        players = new List<PlayerScript>(IngameData.Instance.Players);

        GeneratePlates();
        GenerateTurnQueue();

        // ��������� ������� ��� ������� ������
        PlayerScript firstPlayer = turnsQueue[actualTurnPosition];
        GameObject firstPlayerPlate = playerPlates[firstPlayer];
        PlateAnimationController firstPlateController = firstPlayerPlate.GetComponent<PlateAnimationController>();

        if (firstPlateController != null)
        {
            firstPlateController.OnTurnStart();
        }

        OnPlayerChange?.Invoke(turnsQueue[actualTurnPosition]);
    }

    void AssignRoles()
    {
        int randomLeaderIndex = UnityEngine.Random.Range(0, players.Count); // ���������� ������ ��� ��������

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

        // ��������� ��������
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
        PlayerScript previousPlayer = turnsQueue[actualTurnPosition];

        // ��������� �����
        if (actualTurnPosition + 1 < turnsQueue.Count)
        {
            actualTurnPosition++;
        }
        else
        {
            actualTurnPosition = 0;
        }

        PlayerScript currentPlayer = turnsQueue[actualTurnPosition];

        // ��������� ������ ������������ �� ��������� ������
        GameObject previousPlate = playerPlates[previousPlayer];
        GameObject currentPlate = playerPlates[currentPlayer];

        // ������ ������ ����������� ����������
        PlateAnimationController previousPlateController = previousPlate.GetComponent<PlateAnimationController>();
        PlateAnimationController currentPlateController = currentPlate.GetComponent<PlateAnimationController>();

        if (previousPlateController != null)
        {
            previousPlateController.OnTurnEnd();
        }

        if (currentPlateController != null)
        {
            currentPlateController.OnTurnStart();
        }

        // ������ ��䳿 ���� ������
        OnPlayerChange?.Invoke(currentPlayer);
    }

    void GeneratePlates()
    {
        foreach (PlayerScript player in players)
        {
            GameObject playerPlate = Instantiate(platePrefab, playersPanel.transform);

            // ������������ ������
            PlateCustomization customization = playerPlate.GetComponent<PlateCustomization>();
            customization.CustomizePlate(player);

            if (player.role == PlayerRole.Leader)
            {
                customization.leaderCrown.SetActive(true);
            }

            // ��������� �� ��������
            playerPlates.Add(player, playerPlate);
        }
    }
}
