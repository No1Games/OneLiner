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

    [SerializeField] TutorialManager tutorialManager;

    [Header("Параметри гри")]
    private List<string> wordsForRound;
    private string leaderWord;
    [SerializeField] private Timer timer;





    private void Awake()
    {
        if (IngameData.Instance.IsTutorialOn)
        {
            wordsForRound = wordManager.GetTutorialWords();
            leaderWord = wordManager.GetTutorialLeaderWord();
        }
        else
        {
            tutorialManager.gameObject.SetActive(false);
            wordsForRound = wordManager.FormWordListForRound();
            leaderWord = wordManager.GetLeaderWord(wordsForRound);
        }


        uIManager.GenerateWordButtons(wordsForRound);
        uIManager.SetLeaderWord(leaderWord);
        uIManager.OnGameEnded += CountScore;

        drawingUpdate.OnScreenshotTaken += uIManager.ConfirmLine;

        drawingManager.OnLineUnavailable += uIManager.WarningActivate;
        drawingManager.OnDrawingComplete += uIManager.CallCheckUpMenu;


        uIManager.OnActionConfirmed += playerManager.ChangeTurn;

        playerManager.OnPlayerChange += UpdateCurrentPlayer;
        EnablingTimer();

    }

    private void EnablingTimer()
    {
        if (IngameData.Instance.IsTimerOn)
        {
            timer.SetMaxTime(IngameData.Instance.TimerDuration);
            uIManager.OnTurnStarted += timer.TimerStrat;
            timer.OnTimerEnds += playerManager.ChangeTurn;
            timer.OnTimerEnds += uIManager.OpenPlayerScreen;
        }
        else
        {
            timer.gameObject.SetActive(false);
        }
    }

    private void UpdateCurrentPlayer(PlayerScript currentPlayer)
    {
        uIManager.playerToTrack = currentPlayer;
    }

    private int CountScore()
    {
        timer.PauseTimer();
        return drawingManager.drawenLines;


    }
}
