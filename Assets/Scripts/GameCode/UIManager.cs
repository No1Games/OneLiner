using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject endPanel; // Панель кінця ходу
    [SerializeField] private GameObject wordPanel; // Панель вибору слова
    [SerializeField] private GameObject settingsPanel; // Панель налаштувань
    [SerializeField] private GameObject inGamePanel; // Панель гри
    [SerializeField] private GameObject drawingPanel; // Панель малювання
    [SerializeField] private GameObject endgamePanel; // Панель кінця гри
    [SerializeField] private GameObject warningPanel; // Панель попередження
    [SerializeField] private GameObject confirmedDrawing; // Панель підтвердженого малюнка

    [SerializeField] private GameObject drawingConfirmPanel; // Панель підтвердження лінії малюнка

    private PlayerRole roleKnowTheWord; // Роль гравця, який знає слово

    [Header("Texts")]
    [SerializeField] private TMP_Text drawenLines; // Текст із кількістю намальованих ліній

    [SerializeField] private LocalizeStringEvent warningLocalizeStringEvent; // Локалізований текст попередження

    [Header("Other")]
    public int lives = 2; // Кількість життів гравця
    [SerializeField] private List<GameObject> hearths; // Список сердець для відображення життів

    [SerializeField] private DrawingManager drawing; // Менеджер малювання
    public PlayerScript playerToTrack; // Гравець, за яким стежимо
    private string leaderWordText; // Слово лідера
    private GameObject leaderWord; // Кнопка слова лідера

    private Coroutine warningCoroutine = null; // Поточний корутін попередження
    private bool isWarningActive = false; // Чи активне попередження

    [SerializeField] private GameObject mainCam; // Основна камера

    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab; // Префаб кнопки слова
    private List<GameObject> wordsButtons = new(); // Список створених кнопок слів

    public event Action OnActionConfirmed; // Подія після підтвердження дії
    public event Func<int> OnGameEnded; // Подія завершення гри
    public event Action OnTurnStarted; // Подія початку ходу

    public event Action<bool> OnTutorialWordCheck; // Подія перевірки слова в туторіалі

    private void Start()
    {
        roleKnowTheWord = IngameData.Instance.RoleKnowsWord; // Отримуємо роль, хто знає слово
        OpenPlayerScreen(); // Відкриваємо екран гравця на початку
        InitiateLeaderWordButton(); // Ініціалізуємо кнопку слова лідера
    }

    private void Update()
    {
        drawenLines.text = drawing.drawenLines.ToString(); // Оновлюємо кількість намальованих ліній
    }

    public void GenerateWordButtons(List<string> words)
    {
        // Створюємо кнопки для кожного слова
        for (int i = 0; i < words.Count; i++)
        {
            GameObject newButton = Instantiate(wordButtonPrefab, wordPanel.transform);
            WordButton_new wordInfo = newButton.GetComponent<WordButton_new>();
            wordInfo.SetKey(words[i]);

            // Додаємо відповідну логіку для туторіалу чи звичайної гри
            if (IngameData.Instance.IsTutorialOn)
                wordInfo.wordClicked += TutorialCheckTheWord;
            else
                wordInfo.wordClicked += CheckTheWord;

            wordsButtons.Add(newButton);
        }
    }

    public void SetLeaderWord(string word)
    {
        leaderWordText = word; // Задаємо слово лідера
    }

    // Знаходимо кнопку з словом лідера для відображення/приховування
    private void InitiateLeaderWordButton()
    {
        foreach (GameObject wordButton in wordsButtons)
        {
            if (wordButton.GetComponent<WordButton_new>().GetKey() == leaderWordText)
            {
                Debug.Log("leader word was found");
                leaderWord = wordButton;
                break;
            }
        }
    }

    public void OpenPlayerScreen()
    {
        // Відкриваємо екран гравця: закриваємо панель малювання та вибору слова, якщо вони відкриті
        if (drawingPanel.activeSelf)
            StopDrawing();
        if (wordPanel.activeSelf)
            CloseWordsMenu();

        OnTurnStarted?.Invoke(); // Викликаємо подію початку ходу
        AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_TurnChange);
    }

    public void CallCheckUpMenu()
    {
        // Відкриваємо панель перевірки слова
        endPanel.SetActive(true);
        drawing.DrawingAllowed = false; // Забороняємо малювання
    }

    public void RestartTurn()
    {
        // Відновлюємо хід після помилки: прибираємо останню лінію
        endPanel.SetActive(false);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void ConfirmLine(Texture2D texture)
    {
        // Підтверджуємо лінію малюнка та створюємо спрайт із текстури
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        endPanel.SetActive(false);
        OnActionConfirmed.Invoke();
        OpenPlayerScreen();
        confirmedDrawing.GetComponent<Image>().sprite = screenshotSprite; // Встановлюємо підтверджене зображення
    }

    public void OpenWordsMenu()
    {
        // Відкриваємо меню вибору слова
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WordMenu);
        drawing.DrawingAllowed = false; // Забороняємо малювання під час вибору
        wordPanel.SetActive(true);

        // Відображаємо слово лідера лише для гравця, який його знає
        if (playerToTrack.role == roleKnowTheWord)
            leaderWord.GetComponent<WordButton_new>().ShowLeaderWord();
        else
            leaderWord.GetComponent<WordButton_new>().HideLeaderWord();
    }

    public void CloseWordsMenu()
    {
        // Закриваємо меню вибору слова
        wordPanel.SetActive(false);
    }

    private void CheckTheWord(WordButton_new word)
    {
        // Перевіряємо правильність слова для гравця, який не знає слово
        if (playerToTrack.role != roleKnowTheWord)
        {
            wordPanel.SetActive(false);

            if (word.GetKey() == leaderWordText)
            {
                // Слово правильне – кінець гри з перемогою
                endgamePanel.SetActive(true);
                int _score = OnGameEnded.Invoke();
                endgamePanel.GetComponent<EndGameScreen>().SetFinaleScreen(leaderWordText, true, _score);
            }
            else
            {
                // Слово неправильне – зменшуємо життя та відображаємо анімацію
                lives--;
                hearths[lives].GetComponent<Animator>().Play("EmptyHeartAnimation");

                if (lives >= 1)
                {
                    AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WrongWord);
                    word.ShowWordAsWrong();
                    OnActionConfirmed.Invoke();
                    OpenPlayerScreen();
                }
                else
                {
                    // Життів не залишилось – кінець гри
                    endgamePanel.SetActive(true);
                    int _score = OnGameEnded.Invoke();
                    endgamePanel.GetComponent<EndGameScreen>().SetFinaleScreen(leaderWordText, false);
                }
            }
        }
    }

    private void TutorialCheckTheWord(WordButton_new word)
    {
        // Логіка перевірки слова в туторіалі
        wordPanel.SetActive(false);
        if (word.GetKey() == leaderWordText)
        {
            OnTutorialWordCheck?.Invoke(true); // Слово правильне
            endgamePanel.SetActive(true);
            CloseWordsMenu();
            int _score = OnGameEnded.Invoke();
            endgamePanel.GetComponent<EndGameScreen>().SetFinaleScreen(leaderWordText, true, _score);
        }
        else
        {
            OnTutorialWordCheck?.Invoke(false); // Слово неправильне
            lives--;
            AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WrongWord);
            hearths[lives].GetComponent<Animator>().Play("EmptyHeartAnimation");
            word.ShowWordAsWrong();
        }
    }

    private IEnumerator PushWarning(string key)
    {
        // Показуємо попередження на 2 секунди
        isWarningActive = true;
        warningLocalizeStringEvent.StringReference.TableEntryReference = key;
        warningPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        warningPanel.SetActive(false);
        isWarningActive = false;
    }

    public void WarningActivate(string message)
    {
        // Активуємо попередження, зупиняючи попередній корутін, якщо активний
        if (isWarningActive)
        {
            StopCoroutine(warningCoroutine);
            isWarningActive = false;
        }
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WrongDrawing);
        warningCoroutine = StartCoroutine(PushWarning(message));
    }

    public void OpenSettings()
    {
        // Відкриваємо панель налаштувань
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_Settings);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Закриваємо панель налаштувань
        settingsPanel.SetActive(false);
    }

    public void StartDrawing()
    {
        // Починаємо малювання
        OpenDrawingMenu();
        drawing.DrawingAllowed = true;
    }

    public void OpenDrawingMenu()
    {
        // Відкриваємо меню малювання
        Debug.Log("menu is open to draw");
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_DrawMenu);
        mainCam.SetActive(false);
        drawingPanel.SetActive(true);
    }

    public void StopDrawing()
    {
        // Зупиняємо малювання та повертаємо камеру
        if (drawingConfirmPanel.activeSelf)
            RestartTurn();
        drawingPanel.SetActive(false);
        mainCam.SetActive(true);
        drawing.DrawingAllowed = false;
    }
}
