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
    [SerializeField] private TMP_Text playerNameOnPlayerScreen;

    [SerializeField] private TMP_Text endgameText;
    [SerializeField] private TMP_Text finaleScoreText;
    [SerializeField] private TMP_Text warningText;




    [Header("Other")]
    public int lives = 2;
    [SerializeField] private Animator[] livesImage;
    [SerializeField] private DrawingManager drawing;
    public PlayerScript playerToTrack;
    private string leaderWordText;
    private GameObject leaderWord;

    private Coroutine warningCoroutine = null;
    private bool isWarningActive = false;

    private List<Rank> ranks;
    [SerializeField] private Image rankImage;
    [SerializeField] private GameObject mainCam;

    


    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab;
    private List<GameObject> wordsButtons = new();

    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject menuButton;


    public event Action OnActionConfirmed;
    public event Func<int> OnGameEnded;
    public event Action OnTurnStarted;




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
            WordButton_new wordInfo = newButton.GetComponent<WordButton_new>();
            wordInfo.SetWord(words[i]);
            wordInfo.wordClicked += CheckTheWord;
            wordsButtons.Add(newButton);

            Button button = newButton.GetComponentInChildren<Button>();
            
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

    public void OpenPlayerScreen() //���� ��������� ����� �� ���������� ��� �� ������ ���
    {
        
        //playerNameOnGameScreen.text = playerToTrack.name;
        playerNameOnPlayerScreen.text = playerToTrack.name;
        if (drawingPanel.activeSelf)
        {

            StopDrawing();
        }
        startPanel.SetActive(true);
        


    }

    public void StartTurn() //���� ���������� �� ������� � ������
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_TurnChange);
        startPanel.SetActive(false);
        
        OnTurnStarted?.Invoke();
    }

    public void CallCheckUpMenu() //���� ���������� ���
    {
        endPanel.SetActive(true);
        drawing.DrawingAllowed = false;
    }

    public void RestartTurn()//���� ������� �� ��� �� ��������
    {
        endPanel.SetActive(false);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void ConfirmLine(Texture2D texture)//���� ������� �� ��� ��������
    {
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        endPanel.SetActive(false);
        OnActionConfirmed.Invoke();
        OpenPlayerScreen();
        confirmedDrawing.GetComponent<Image>().sprite = screenshotSprite;
    }

    public void OpenWordsMenu()//���� ������ ���������� ����� ��� ��� ����� �� �������
    {

        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WordMenu);
        drawing.DrawingAllowed = false;
        wordPanel.SetActive(true);

        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderWord.GetComponent<WordButton_new>().ShowLeaderWord();
        }



    }

    public void CloseWordsMenu() //���� ������ ����������� �� ���������
    {
        wordPanel.SetActive(false);
        

        //drawing.DrawingAllowed = true;
        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderWord.GetComponent<WordButton_new>().HideLeaderWord();


        }
    }

    private void CheckTheWord(WordButton_new word)
    {

        string winingText = "³��� � ���������";
        string losingText = "������, ����� ���� �� ��������";


        if (playerToTrack.role != PlayerRole.Leader)
        {
            wordPanel.SetActive(false);
            if (word.gameObject.GetComponentInChildren<TMP_Text>().text == leaderWordText)
            {

                endgameText.text = winingText;
                endgamePanel.SetActive(true);
                AudioManager.Instance.TurnMusicOff();
                AudioManager.Instance.PlaySoundInMain(GameSounds.Game_Victory);
                StartCoroutine(StartScoring(OnGameEnded.Invoke()));

            }
            else
            {
                if (lives > 1)
                {
                    AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WrongWord);
                    lives--;
                    word.ShowWordAsWrong();

                   //livesImage[0].SetTrigger("BeGray"); //exception null ref
                    
                    OnActionConfirmed.Invoke();
                    OpenPlayerScreen();

                }
                else
                {
                    endgameText.text = losingText;
                    endgamePanel.SetActive(true);
                    AudioManager.Instance.TurnMusicOff();
                    AudioManager.Instance.PlaySoundInMain(GameSounds.Game_Lose);
                    StartCoroutine(StartScoring(0));

                }

            }

        }


    }

    public void BackToMainMenu()
    {
        AudioManager.Instance.TurnMusicOn();
        AudioManager.Instance.StopSoundInAdditional();
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        AudioManager.Instance.TurnMusicOn();
        AudioManager.Instance.StopSoundInAdditional();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);

    }


    private IEnumerator StartScoring(int finaleScore)
    {
        int scoreNow = 0;
        int rankIndex = 0;
        Rank currentRank = ranks[0];  // ���������� ����
        rankImage.gameObject.SetActive(true);  // ³��������� ���������� �����
        rankImage.sprite = currentRank.rankImage;  // ������������ ���������� ������

        while (scoreNow < finaleScore)
        {
            scoreNow += 10;  // �������� ���� ���������

            // ��������� ����� ��� �������� ������
            finaleScoreText.text = scoreNow.ToString();  // ��������� ����� � ������

            // ����������, �� ��������� �������� �������� ��� ���������� �����
            if (rankIndex < ranks.Count - 1 && scoreNow >= ranks[rankIndex + 1].threshold)
            {
                rankIndex++;  // ������� ������ �����
                currentRank = ranks[rankIndex];  // ��������� �������� ����
                rankImage.sprite = currentRank.rankImage;  // ̳����� ���������� �����
                yield return new WaitForSeconds(0.5f);  // �������� ��� ���������� ���� �����
            }

            yield return null;  // ������� �� ���� ���� ����� ������������
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
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WrongDrawing);
        warningCoroutine = StartCoroutine(PushWarning(message));
    }

    public void OpenSettings()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_Settings);
        settingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void StartDrawing()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_DrawMenu);
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
