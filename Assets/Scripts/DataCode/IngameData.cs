using System.Collections.Generic;
using Unity.VisualScripting;
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
        set => timerDuration = value;
    }

    // Role-specific settings
    private PlayerRole roleKnowsWord;
    public PlayerRole RoleKnowsWord
    {
        get => roleKnowsWord;
        set => roleKnowsWord = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Method to reset IngameData when returning to the main menu
    public void ResetData()
    {
        players.Clear();
        isTimerOn = true;
        timerDuration = 60;
        roleKnowsWord = PlayerRole.NotSetYet;
    }

    // Method to initialize players
    public void InitializePlayers(List<PlayerScript> newPlayers)
    {
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

}


