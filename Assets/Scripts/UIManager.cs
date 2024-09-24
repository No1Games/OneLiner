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
    [SerializeField] private GameObject leaderPanel;
    [SerializeField] private GameObject ingamePanel;
    [SerializeField] private GameObject endgamePanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text drawenLines;
    [SerializeField] private TMP_Text playerNameOnGameScreen;
    [SerializeField] private TMP_Text playerNameOnPlayerScreen;
    [SerializeField] private TMP_Text leaderWord;
    [SerializeField] private TMP_Text endgameText;
    [SerializeField] private TMP_Text finaleScoreText;



    [Header("Other")]
    public int lives = 2;
    [SerializeField] private GameObject[] livesImage;
    [SerializeField] private DrawingManager drawing;
    public PlayerScript playerToTrack;

    private List<Rank> ranks;
    [SerializeField] private Image rankImage;




    [Header("Buttons")]
    [SerializeField] private GameObject wordButtonPrefab;
    private List<GameObject> wordsButtons = new();


    public event Action actionConfirmed;
    public event Func<int> gameEnded;




    private void Start()
    {

        OpenPlayerScreen();

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
        leaderWord.text = word;
    }

    public void OpenPlayerScreen() //���� ��������� ����� �� ���������� ���
    {
        playerNameOnGameScreen.text = playerToTrack.name;
        playerNameOnPlayerScreen.text = playerToTrack.name;

        startPanel.SetActive(true);
        endPanel.SetActive(false);
        ingamePanel.SetActive(false);
        drawing.DrawingAllowed = false;

    }

    public void StartTurn() //���� ���������� �� ������� � ������
    {
        startPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.DrawingAllowed = true;
    }

    public void CallCheckUpMenu() //���� ���������� ���
    {
        endPanel.SetActive(true);
        ingamePanel.SetActive(false);
        drawing.DrawingAllowed = false;
    }

    public void RestartTurn()//���� ������� �� ��� �� ��������
    {
        endPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.RemoveLastLine();
        drawing.DrawingAllowed = true;
    }

    public void ConfirmLine()//���� ������� �� ��� ��������
    {
        actionConfirmed.Invoke();
        OpenPlayerScreen();

    }

    public void OpenWordsMenu()//���� ������ ���������� ����� ��� ��� ����� �� �������
    {

        ingamePanel.SetActive(false);
        drawing.DrawingAllowed = false;

        if (playerToTrack.role == PlayerRole.Leader)
        {
            leaderPanel.SetActive(true);
        }
        else
        {
            wordPanel.SetActive(true);

        }

    }

    public void CloseWordsMenu() //���� ������ ����������� �� ���������
    {
        wordPanel.SetActive(false);
        leaderPanel.SetActive(false);
        ingamePanel.SetActive(true);
        drawing.DrawingAllowed = true;
    }

    private void CheckTheWord(string buttonText)
    {

        string winingText = "³��� � ���������";
        string losingText = "������, ����� ���� �� ��������";
        wordPanel.SetActive(false);

        if (buttonText == leaderWord.text)
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
                yield return new WaitForSeconds(0.1f);  // �������� ��� ���������� ���� �����
            }

            yield return null;  // ������� �� ���� ���� ����� ������������
        }

        // �������� ������ "����������" �� "����������� �� ����"
        //replayButton.SetActive(true);
        //menuButton.SetActive(true);
    }

}
