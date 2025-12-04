using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] PlayerManager playerManager; // Менеджер гравців
    [SerializeField] WordManager wordManager; // Менеджер слів
    [SerializeField] UIManager uIManager; // Менеджер UI
    [SerializeField] DrawingManager drawingManager; // Менеджер малювання
    [SerializeField] DrawingUpdate drawingUpdate; // Менеджер для створення скріншотів малюнків

    [SerializeField] TutorialManager tutorialManager; // Менеджер туторіалу

    [Header("Параметри гри")]
    private List<string> wordsForRound; // Слова для поточного раунду
    private string leaderWord; // Слово лідера
    [SerializeField] private Timer timer; // Таймер для ходу

    private void Awake()
    {
        // Ініціалізація слів для раунду та слова лідера
        if (IngameData.Instance.IsTutorialOn)
        {
            wordsForRound = wordManager.GetTutorialWords(); // Слова для туторіалу
            leaderWord = wordManager.GetTutorialLeaderWord(); // Слово лідера для туторіалу
        }
        else
        {
            tutorialManager.gameObject.SetActive(false); // Вимикаємо туторіал, якщо він не активний
            wordsForRound = wordManager.FormWordListForRound(); // Формуємо список слів для раунду
            leaderWord = wordManager.GetLeaderWord(wordsForRound); // Визначаємо слово лідера
        }

        // Генеруємо кнопки слів у UI та встановлюємо слово лідера
        uIManager.GenerateWordButtons(wordsForRound);
        uIManager.SetLeaderWord(leaderWord);

        // Підписуємося на події
        uIManager.OnGameEnded += CountScore; // Підрахунок очок по завершенні гри
        drawingUpdate.OnScreenshotTaken += uIManager.ConfirmLine; // Підтвердження малюнка після скріншоту
        drawingManager.OnLineUnavailable += uIManager.WarningActivate; // Попередження про недоступність лінії
        drawingManager.OnDrawingComplete += uIManager.CallCheckUpMenu; // Виклик перевірки після завершення малювання
        uIManager.OnActionConfirmed += playerManager.ChangeTurn; // Зміна ходу після підтвердження дії
        playerManager.OnPlayerChange += UpdateCurrentPlayer; // Оновлення поточного гравця в UI

        EnablingTimer(); // Налаштування таймера
    }

    private void EnablingTimer()
    {
        // Активуємо таймер, якщо він увімкнений
        if (IngameData.Instance.IsTimerOn)
        {
            timer.SetMaxTime(IngameData.Instance.TimerDuration); // Встановлюємо максимальний час ходу
            uIManager.OnTurnStarted += timer.TimerStrat; // Старт таймера на початку ходу
            timer.OnTimerEnds += playerManager.ChangeTurn; // Зміна ходу після закінчення таймера
            timer.OnTimerEnds += uIManager.OpenPlayerScreen; // Відкриття екрану гравця після закінчення таймера
        }
        else
        {
            timer.gameObject.SetActive(false); // Вимикаємо таймер, якщо він не використовується
        }
    }

    private void UpdateCurrentPlayer(PlayerScript currentPlayer)
    {
        // Оновлюємо гравця, за яким стежить UI
        uIManager.playerToTrack = currentPlayer;
    }

    private int CountScore()
    {
        // Зупиняємо таймер і повертаємо кількість намальованих ліній як очки
        timer.PauseTimer();
        return drawingManager.drawenLines;
    }
}
