using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] PlayerManager playerManager;
    [SerializeField] WordManager wordManager;
    [SerializeField] UIManager uIManager;
    [SerializeField] DrawingManager drawingManager;
    [SerializeField] DrawingUpdate drawingUpdate;

    [Header("Параметри гри")]
    private List<string> wordsForRound;
    private string leaderWord;


    [Header("Підрахунок ПО")]
    [SerializeField] private List<Rank> gameRanks;


    private void Awake()
    {
        wordsForRound = wordManager.FormWordListForRound();
        leaderWord = wordManager.GetLeaderWord(wordsForRound);
        uIManager.GenerateWordButtons(wordsForRound);
        uIManager.SetLeaderWord(leaderWord);
        uIManager.gameEnded += CountScore;

        drawingUpdate.OnScreenshotTaken += uIManager.ConfirmLine;

        drawingManager.OnLineUnavailable += uIManager.WarningActivate;
        drawingManager.OnDrawingComplete += uIManager.CallCheckUpMenu;

        
        uIManager.actionConfirmed += playerManager.ChangeTurn;

        playerManager.OnPlayerChange += UpdateCurrentPlayer;

        uIManager.RanksSet(gameRanks);


    }



    private void UpdateCurrentPlayer(PlayerScript currentPlayer)
    {
        uIManager.playerToTrack = currentPlayer;
    }

    private int CountScore()
    {
        int maxPoints = 1000;
        int heartPoints = 100;
        int linePoints = 20;
        int score = maxPoints - drawingManager.drawenLines * linePoints + uIManager.lives * heartPoints; 
        return score;

    }
}
