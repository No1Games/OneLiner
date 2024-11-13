using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OnlineGameSetup : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    private int _maxHearts = 2;
    private int _currentHearts;

    [SerializeField] private StartTurnPanelUI _startTurnPanel;
    [SerializeField] private TurnHandler _turnHandler;

    [SerializeField] private WordsPanel _wordsPanel;
    [SerializeField] private WordManager _wordManager;
    private List<string> _words = new List<string>();
    private List<int> _wordsIndexes = new List<int>();
    private int _leaderWord;

    [SerializeField] private NGODrawManager _drawingManager;
    [SerializeField] private DrawingUpdate _drawingUpdate;
    [SerializeField] private Image _drawnImage;

    [SerializeField] private Camera _mainCamera;

    [SerializeField] private GameOverUI _gameOverUI;

    [SerializeField] private HeartsUI _heartsUI;

    private NetworkGameManager _networkGameManager;

    private LocalLobby _localLobby;
    private LocalPlayer _localPlayer;

    private void Start()
    {
        _currentHearts = _maxHearts;
        _heartsUI.Init(_currentHearts);

        _localLobby = GameManager.Instance.LocalLobby;
        _localPlayer = GameManager.Instance.LocalUser;

        _networkGameManager = FindAnyObjectByType<NetworkGameManager>();

        _localLobby.WordsList.onChanged += OnWordsChanged;
        _localLobby.LeaderWord.onChanged += OnLeaderWordChanged;

        if (_localPlayer.Role.Value == PlayerRole.Leader)
        {
            _words = _wordManager.FormWordListForRound();
            _wordsIndexes = _wordManager.GetWordsIndexes(_words);
            _leaderWord = _wordManager.GetLeaderWordIndex(_words);
            GameManager.Instance.SetWordsList(_wordsIndexes, _leaderWord);
        }

        _drawingManager.OnLineConfirmed += OnLineConfirmed;

        _drawingManager.OnLineSpawned += OnLineSpawned;

        _drawingUpdate.OnScreenshotTaken += OnScreenshotTaken;

        _wordsPanel.UserClickedWord += OnUserMakeGuess;
    }

    private void OnDestroy()
    {
        _localLobby.WordsList.onChanged -= OnWordsChanged;
        _localLobby.LeaderWord.onChanged -= OnLeaderWordChanged;

        _drawingUpdate.OnScreenshotTaken -= OnScreenshotTaken;
    }

    private void OnLineSpawned()
    {
        ToggleScreen(true);
        _drawingUpdate.TakeScreenshot();
        ToggleScreen(false);
    }

    private void OnLineConfirmed(NGOLine line)
    {
        _networkGameManager.SpawnLine(line.Start, line.End);
        _turnHandler.EndTurn();
    }

    private void OnWordsChanged(List<int> indexes)
    {
        _words = _wordManager.GetWordsFromIndexes(indexes);

        _wordsPanel.SetButtons(_words);
    }

    private void OnLeaderWordChanged(int index)
    {
        _leaderWord = index;

        _wordsPanel.SetLeaderWord(index);
    }

    private void UpdatePicture(Texture2D texture)
    {
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        _drawnImage.sprite = screenshotSprite;
    }

    private void OnScreenshotTaken(Texture2D texture)
    {
        UpdatePicture(texture);

        ToggleScreen(false);
    }

    public void ToggleScreen(bool isDrawing)
    {
        _drawingUpdate.gameObject.SetActive(isDrawing);
        _mainCamera.gameObject.SetActive(!isDrawing);
        _drawingManager.IsDrawAllowed = isDrawing;
    }

    private void OnUserMakeGuess(int index)
    {
        if (_leaderWord != index)
        {
            _logger.Log("Oh-oh! Wrong word! Duh!");
            int newHearts = _currentHearts - 1;

            Debug.Log($"Old hearts count: {_currentHearts} --- New hearts count: {newHearts}");

            if (newHearts <= 0)
            {
                _networkGameManager.OnGameOver(false);
            }
            else
            {
                _networkGameManager.OnGuessedWrong(index, newHearts);
            }

            _turnHandler.EndTurn();
        }
        else
        {
            _networkGameManager.OnGameOver(true, 100f);
        }
    }

    public void SetHearts(int count)
    {
        _currentHearts = count;

        _heartsUI.UpdateHearts(count);
    }

    public void ShowGameOverScreen(bool isWin, float score = 0)
    {
        _gameOverUI.Show(isWin, score);
    }

    public void DisableButtonByIndex(int index)
    {
        _wordsPanel.DisableButton(index);
    }
}
