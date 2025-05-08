using System.Collections.Generic;
using UnityEngine;

public class IngameData : MonoBehaviour
{
    // Singleton instance
    public static IngameData Instance { get; private set; }

    // Players list
    private List<PlayerScript> players = new List<PlayerScript>();
    public List<PlayerScript> Players
    {
        get => players;
        set => players = value;
    }
    // Timer settings
    private bool isTutorialOn;
    public bool IsTutorialOn
    {
        get => isTutorialOn;
        set => isTutorialOn = value;
    }
    

    // Timer settings
    private bool isTimerOn;
    public bool IsTimerOn
    {
        get => isTimerOn;
        set => isTimerOn = value;
    }

    private int timerDuration;
    public int TimerDuration
    {
        get => timerDuration;
        private set => timerDuration = value;
    }

    // Role-specific settings
    private PlayerRole roleKnowsWord;
    public PlayerRole RoleKnowsWord
    {
        get => roleKnowsWord;
        private set => roleKnowsWord = value;
    }

    // Game mode
    private GameModes gameMode; // Назва ігрового режиму
    public GameModes GameMode
    {
        get => gameMode;
        private set => gameMode = value;
    }

    // Flag for returning from the game
    public bool ReturnedFromGame { get; private set; }
    public void SetReturnedFromGame(bool value)
    {
        ReturnedFromGame = value;
    }

    // Singleton setup
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetData();
    }

    // Method to reset IngameData when returning to the main menu
    public void ResetData()
    {
        players.Clear();
        isTimerOn = true;
        timerDuration = 60; // Базова тривалість таймера
        roleKnowsWord = PlayerRole.NotSetYet;
        gameMode = GameModes.Coop; // Базовий ігровий режим
        ReturnedFromGame = false;
    }

    // Method to initialize players
    public void InitializePlayers(List<PlayerScript> newPlayers)
    {
        players.Clear();
        Players = newPlayers;
    }

    // Method to initialize timer
    public void InitializeTimer(bool timerOn, int duration)
    {
        IsTimerOn = timerOn;
        TimerDuration = duration;
    }

    // Method to set role information
    public void SetRoleKnowsWord(PlayerRole role)
    {
        RoleKnowsWord = role;
    }

    // Method to set the game mode
    public void SetGameMode(GameModes mode)
    {
        GameMode = mode;
    }
}
