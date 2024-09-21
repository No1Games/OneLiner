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

    [Header("Параметри гри")]
    private List<string> wordsForRound;
    private string leaderWord;



    private void Awake()
    {
        wordsForRound = wordManager.FormWordListForRound();
        leaderWord = wordManager.GetLeaderWord(wordsForRound);
        uIManager.GenerateWordButtons(wordsForRound);
        uIManager.SetLeaderWord(leaderWord);


        
        drawingManager.OnDrawingComplete += uIManager.CallCheckUpMenu;

        
        uIManager.actionConfirmed += playerManager.ChangeTurn;

        playerManager.OnPlayerChange += UpdateCurrentPlayer;


    }



    private void UpdateCurrentPlayer(PlayerScript currentPlayer)
    {
        uIManager.playerToTrack = currentPlayer;
    }
}
