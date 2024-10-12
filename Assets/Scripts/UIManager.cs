using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject wordPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject drawingPanel;
    [SerializeField] private GameObject endgamePanel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject confirmedDrawing;




    [Header("Texts")]
    [SerializeField] private TMP_Text drawenLines;
    [SerializeField] private TMP_Text playerNameOnGameScreen;
    [SerializeField] private TMP_Text playerNameOnPlayerScreen;

    [SerializeField] private TMP_Text endgameText;
    [SerializeField] private TMP_Text finaleScoreText;
    [SerializeField] private TMP_Text warningText;




    [Header("Other")]
    public int lives = 2;
    [SerializeField] private GameObject[] livesImage;
    [SerializeField] private DrawingManager drawing;
    public PlayerScript playerToTrack;
    private string leaderWordText;
    private GameObject leaderWord;

    private Coroutine warningCoroutine = null;
    private bool isWarningActive = false;

    private List<Rank> ranks;
    [SerializeField] private Image rankImage;
    [SerializeField] private GameObject mainCam;

    public Sprite test;


    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab;
    private List<GameObject> wordsButtons = new();

    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject menuButton;


    public event Action actionConfirmed;
    public event Func<int> gameEnded;




    private void Start()
    {

        OpenPlayerScreen();
        InitiateLeaderWordButton();

    }
    private void Update()
    {
        drawenLines.text = drawing.drawenLines.ToString();
    }
    public void RanksSet(List<Rank> ranks)
    {
        this.ranks = ranks;
    }



    public void GenerateWordButtons(List<string> words)
    {
        for (int i = 0; i < words.Count; i++)
        {

            GameObject newButton = Instantiate(wordButtonPrefab, wordPanel.transform);
            newButton.GetComponentInChildren<TMP_Text>().text = words[i];
            wordsButtons.Add(newButton);

            Button button = newButton.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => CheckTheWord(newButton.GetComponentInChildren<TMP_Text>().text));
        }
    }


    public void SetLeaderWord(string word)
    {
        leaderWordText = word;


    }
    private void InitiateLeaderWordButton()
    {
        foreach (GameObject wordButton in wordsButtons)
        {
            if (wordButton.GetComponentInChildren<TMP_Text>().text == leaderWordText)
            {
                Debug.Log("leader finded");
                leaderWord = wordButton;
                break;
            }
        }

    }

    public void OpenPlayerScreen() //коли вгадували слово чи підтвердили лінію
    {
        playerNameOnGameScreen.text = playerToTrack.name;
        playerNameOnPlayerScreen.text = playerToTrack.name;
        if (drawingPanel.activeSelf)
        {

            StopDrawing();
        }
        startPanel.SetActive(true);
        inGamePanel.SetActive(false);


    }

    public void StartTurn() //коли підтвердили що телефон у гравця
    {
        startPanel.SetActive(false);
        inGamePanel.SetActive(true);
    }

    public void CallCheckUpMenu() //коли намалювали лінію
    {
        endPanel.SetActive(true);
        drawing.DrawingAllowed = false;
    }

    public void RestartTurn()//коли визнали шо лінія не підходить
    {
        endPanel.SetActive(false);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void ConfirmLine(Texture2D texture)//коли визнали шо лінія підходить
    {
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        endPanel.SetActive(false);
        actionConfirmed.Invoke();
        OpenPlayerScreen();
        confirmedDrawing.GetComponent<Image>().sprite = screenshotSprite;
        //confirmedDrawing.GetComponent<Image>().sprite = test;


    }

    public void OpenWordsMenu()//коли хочемо подивитись слова або своє слово як ведучий
    {

        inGamePanel.SetActive(false);
        drawing.DrawingAllowed = false;
        wordPanel.SetActive(true);

        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderWord.GetComponentInChildren<Image>().color = Color.yellow; //TO DO: choose image
        }



    }

    public void CloseWordsMenu() //коли хочемо повернутись до малювання
    {
        wordPanel.SetActive(false);

        inGamePanel.SetActive(true);
        drawing.DrawingAllowed = true;
        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderWord.GetComponentInChildren<Image>().color = Color.white; //TO DO: choose image


        }
    }

    private void CheckTheWord(string buttonText)
    {

        string winingText = "Вітаю з перемогою";
        string losingText = "Нажаль, цього разу ви програли";


        if (playerToTrack.role != PlayerRole.Leader)
        {
            wordPanel.SetActive(false);
            if (buttonText == leaderWordText)
            {

                endgameText.text = winingText;

                endgamePanel.SetActive(true);
                StartCoroutine(StartScoring(gameEnded.Invoke()));

            }
            else
            {



                if (lives > 0)
                {
                    lives--;
                    livesImage[lives].SetActive(false);
                    actionConfirmed.Invoke();
                    OpenPlayerScreen();

                }
                else
                {
                    endgameText.text = losingText;
                    endgamePanel.SetActive(true);

                }

            }

        }


    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);

    }


    private IEnumerator StartScoring(int finaleScore)
    {
        int scoreNow = 0;
        int rankIndex = 0;
        Rank currentRank = ranks[0];  // Початковий ранг
        rankImage.gameObject.SetActive(true);  // Відображаємо зображення рангу
        rankImage.sprite = currentRank.rankImage;  // Встановлюємо початковий спрайт

        while (scoreNow < finaleScore)
        {
            scoreNow += 10;  // Збільшуємо очки поступово

            // Оновлюємо текст без окремого методу
            finaleScoreText.text = scoreNow.ToString();  // Оновлюємо текст з очками

            // Перевіряємо, чи досягнуто порогове значення для наступного рангу
            if (rankIndex < ranks.Count - 1 && scoreNow >= ranks[rankIndex + 1].threshold)
            {
                rankIndex++;  // Змінюємо індекс рангу
                currentRank = ranks[rankIndex];  // Оновлюємо поточний ранг
                rankImage.sprite = currentRank.rankImage;  // Міняємо зображення рангу
                yield return new WaitForSeconds(0.5f);  // Затримка для візуалізації зміни рангу
            }

            yield return null;  // Перерва на один кадр перед продовженням
        }


        replayButton.SetActive(true);
        menuButton.SetActive(true);
    }

    private IEnumerator PushWarning(string message)
    {
        isWarningActive = true;
        warningText.text = message;
        warningPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        warningPanel.SetActive(false);
        isWarningActive = false;
    }
    public void WarningActivate(string message)
    {
        if (isWarningActive)
        {
            StopCoroutine(warningCoroutine);
            isWarningActive = false;
        }

        warningCoroutine = StartCoroutine(PushWarning(message));
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void StartDrawing()
    {
        mainCam.SetActive(false);
        drawingPanel.SetActive(true);

        drawing.DrawingAllowed = true;

    }
    public void StopDrawing()
    {
        drawingPanel.SetActive(false);
        mainCam.SetActive(true);
        drawing.DrawingAllowed = false;
    }


}
