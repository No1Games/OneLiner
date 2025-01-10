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

        // Запускаємо анімацію для першого гравця
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
        PlayerScript previousPlayer = turnsQueue[actualTurnPosition];

        // Оновлення черги
        if (actualTurnPosition + 1 < turnsQueue.Count)
        {
            actualTurnPosition++;
        }
        else
        {
            actualTurnPosition = 0;
        }

        PlayerScript currentPlayer = turnsQueue[actualTurnPosition];

        // Отримання плашок попереднього та поточного гравця
        GameObject previousPlate = playerPlates[previousPlayer];
        GameObject currentPlate = playerPlates[currentPlayer];

        // Виклик методів анімаційного контролера
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

        // Виклик події зміни гравця
        OnPlayerChange?.Invoke(currentPlayer);
    }

    void GeneratePlates()
    {
        foreach (PlayerScript player in players)
        {
            GameObject playerPlate = Instantiate(platePrefab, playersPanel.transform);

            // Налаштування плашки
            PlateCustomization customization = playerPlate.GetComponent<PlateCustomization>();
            customization.CustomizePlate(player);

            if (player.role == PlayerRole.Leader)
            {
                customization.leaderCrown.SetActive(true);
            }

            // Додавання до словника
            playerPlates.Add(player, playerPlate);
        }
    }
}
