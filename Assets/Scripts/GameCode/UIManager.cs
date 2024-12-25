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
    


    [SerializeField] private TMP_Text warningText;




    [Header("Other")]
    public int lives = 2;
    [SerializeField] private List<GameObject> hearths;

    [SerializeField] private DrawingManager drawing;
    public PlayerScript playerToTrack;
    private string leaderWordText;
    private GameObject leaderWord;

    private Coroutine warningCoroutine = null;
    private bool isWarningActive = false;

    
    [SerializeField] private GameObject mainCam;

    


    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab;
    private List<GameObject> wordsButtons = new();

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

    public void OpenPlayerScreen() //коли вгадували слово чи підтвердили лінію чи вийшов час
    {

        
        if (drawingPanel.activeSelf)
        {

            StopDrawing();
        }
        OnTurnStarted?.Invoke();
        AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_TurnChange);


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
        OnActionConfirmed.Invoke();
        OpenPlayerScreen();
        confirmedDrawing.GetComponent<Image>().sprite = screenshotSprite;
    }

    public void OpenWordsMenu()//коли хочемо подивитись слова або своє слово як ведучий
    {

        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_WordMenu);
        drawing.DrawingAllowed = false;
        wordPanel.SetActive(true);

        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderWord.GetComponent<WordButton_new>().ShowLeaderWord();
        }



    }

    public void CloseWordsMenu() //коли хочемо повернутись до малювання
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

        if (playerToTrack.role != PlayerRole.Leader)
        {
            wordPanel.SetActive(false);
            if (word.gameObject.GetComponentInChildren<TMP_Text>().text == leaderWordText)
            {

                
                endgamePanel.SetActive(true);
                int _score = OnGameEnded.Invoke();
                endgamePanel.GetComponent<EndGameScreen>().SetFinaleScreen(leaderWordText, true, _score);
                

            }
            else
            {
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
                    
                    endgamePanel.SetActive(true);
                    int _score = OnGameEnded.Invoke();
                    endgamePanel.GetComponent<EndGameScreen>().SetFinaleScreen(leaderWordText, false);


                }

            }

        }


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
