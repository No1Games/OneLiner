using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject wordPanel;
    [SerializeField] private GameObject leaderPanel;
    [SerializeField] private GameObject ingamePanel;
    [SerializeField] private GameObject endgamePanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text playerNameOnGameScreen;
    [SerializeField] private TMP_Text playerNameOnPlayerScreen;
    [SerializeField] private TMP_Text leaderWord;
    [SerializeField] private TMP_Text endgameText;



    [Header("Other")]
    private int lives = 2;
    [SerializeField] private GameObject[] livesImage;
    [SerializeField] private DrawingManager drawing;
    public PlayerScript playerToTrack;



    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab;
    private List<GameObject> wordsButtons = new();

    
    public event Action actionConfirmed;
<<<<<<< HEAD
    public event Func<int> gameEnded;
    public event Action<bool> gameScreenIsPerformed;

=======
>>>>>>> parent of aa20503 (Score and some game ending)



    private void Start()
    {

        OpenPlayerScreen();

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
        leaderWord.text = word;
    }

    public void OpenPlayerScreen() //коли вгадували слово чи підтвердили лінію
    {
        playerNameOnGameScreen.text = playerToTrack.name;
        playerNameOnPlayerScreen.text = playerToTrack.name;

        startPanel.SetActive(true);
        endPanel.SetActive(false);
        ingamePanel.SetActive(false);
        gameScreenIsPerformed.Invoke(false);

    }

    public void StartTurn() //коли підтвердили що телефон у гравця
    {
        startPanel.SetActive(false);
        ingamePanel.SetActive(true);
        gameScreenIsPerformed.Invoke(true);
    }

    public void CallCheckUpMenu() //коли намалювали лінію
    {
        endPanel.SetActive(true);
        ingamePanel.SetActive(false);
        gameScreenIsPerformed.Invoke(false);
    }

    public void RestartTurn()//коли визнали шо лінія не підходить
    {
        endPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.RemoveLastLine();
        gameScreenIsPerformed.Invoke(true);
    }

    public void ConfirmLine()//коли визнали шо лінія підходить
    {
        actionConfirmed.Invoke();
        OpenPlayerScreen();

    }

    public void OpenWordsMenu()//коли хочемо подивитись слова або своє слово як ведучий
    {

        ingamePanel.SetActive(false);
        gameScreenIsPerformed.Invoke(false);

        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderPanel.SetActive(true);
        }
        else
        {
            wordPanel.SetActive(true);

        }

    }

    public void CloseWordsMenu() //коли хочемо повернутись до малювання
    {
        wordPanel.SetActive(false);
        leaderPanel.SetActive(false);
        ingamePanel.SetActive(true);
        gameScreenIsPerformed.Invoke(true);
    }

    private void CheckTheWord(string buttonText)
    {

        string winingText = "Вітаю з перемогою";
        string losingText = "Нажаль, цього разу ви програли";
        wordPanel.SetActive(false);

        if (buttonText == leaderWord.text)
        {

            endgameText.text = winingText;
            endgamePanel.SetActive(true);

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

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);

    }

}
