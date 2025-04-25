using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
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

    [SerializeField] private GameObject drawingConfirmPanel;

    private PlayerRole roleKnowTheWord;


    [Header("Texts")]
    [SerializeField] private TMP_Text drawenLines;



    [SerializeField] private LocalizeStringEvent warningLocalizeStringEvent;




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
        roleKnowTheWord = IngameData.Instance.RoleKnowsWord;
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
            wordInfo.SetKey(words[i]);
            wordInfo.wordClicked += CheckTheWord;
            wordsButtons.Add(newButton);

            //Button button = newButton.GetComponentInChildren<Button>();
            
        }
    }
    public void SetLeaderWord(string word)
    {
        leaderWordText = word;


    }
    // знаходимо сам об'єкт з лідер вордом, щоб показувати його або ховати
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

    public void OpenPlayerScreen() //коли вгадували слово чи підтвердили лінію чи вийшов час
    {

        
        if (drawingPanel.activeSelf)
        {

            StopDrawing();
        }
        if (wordPanel.activeSelf)
        {
            CloseWordsMenu();
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

        if (playerToTrack.role == roleKnowTheWord)
        {
            leaderWord.GetComponent<WordButton_new>().ShowLeaderWord();
        }
        else
        {
            leaderWord.GetComponent<WordButton_new>().HideLeaderWord();
        }



    }

    public void CloseWordsMenu() //коли хочемо повернутись до малювання
    {
        
        
        //if (playerToTrack.role == roleKnowTheWord)
        //{
            

        //}
        wordPanel.SetActive(false);
    }

    private void CheckTheWord(WordButton_new word)
    {

        if (playerToTrack.role != roleKnowTheWord)
        {
            
            wordPanel.SetActive(false);
            if (word.gameObject.GetComponent<WordButton_new>().GetKey() == leaderWordText)
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

    private IEnumerator PushWarning(string key)
    {
        
        isWarningActive = true;
        warningLocalizeStringEvent.StringReference.TableEntryReference = key;
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
        OpenDrawingMenu();

        drawing.DrawingAllowed = true;

    }

    public void OpenDrawingMenu()
    {
        Debug.Log("menu is open to draw");
        AudioManager.Instance.PlaySoundInMain(GameSounds.Game_DrawMenu);
        mainCam.SetActive(false);
        drawingPanel.SetActive(true);
    }

    public void StopDrawing()
    {
        if (drawingConfirmPanel.activeSelf)
        {
            RestartTurn();
        }
        drawingPanel.SetActive(false);
        mainCam.SetActive(true);
        drawing.DrawingAllowed = false;
    }


}
