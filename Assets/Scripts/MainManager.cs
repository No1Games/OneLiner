using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        uIManager.gameScreenIsPerformed += AllowDrawing;

        
        uIManager.actionConfirmed += playerManager.ChangeTurn;

        playerManager.OnPlayerChange += UpdateCurrentPlayer;


    }



    private void UpdateCurrentPlayer(PlayerScript currentPlayer)
    {
        uIManager.playerToTrack = currentPlayer;
    }
<<<<<<< HEAD

    private int CountScore()
    {
        int maxPoints = 1000;
        int heartPoints = 100;
        int linePoints = 20;
        int score = maxPoints - drawingManager.drawenLines * linePoints + uIManager.lives * heartPoints; 
        return score;

    }

    private void AllowDrawing(bool isAllowed)
    {
        if (isAllowed)
        {
            drawingManager.gameObject.SetActive(true);
        }
        else
        {
            drawingManager.gameObject.SetActive(false);
        }
    }
=======
>>>>>>> parent of aa20503 (Score and some game ending)
}
