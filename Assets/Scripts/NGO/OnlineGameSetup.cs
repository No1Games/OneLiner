using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OnlineGameSetup : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

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

    private NetworkGameManager _networkGameManager;

    private LocalLobby _localLobby;
    private LocalPlayer _localPlayer;

    private void Start()
    {
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

        _drawingManager.OnDrawingEnd += OnDrawingEnd;

        _drawingUpdate.OnScreenshotTaken += OnDrawingComplete;

        _wordsPanel.UserClickedWord += OnUserMakeGuess;
    }

    private void OnDestroy()
    {
        _localLobby.WordsList.onChanged -= OnWordsChanged;
        _localLobby.LeaderWord.onChanged -= OnLeaderWordChanged;

        _drawingUpdate.OnScreenshotTaken -= OnDrawingComplete;
    }

    private void OnDrawingEnd(NGOLine line)
    {
        _networkGameManager.SpawnLine(line.Start, line.End);
    }

    private void OnWordsChanged(List<int> indexes)
    {
        _words = _wordManager.GetWordsFromIndexes(indexes);

        _wordsPanel.SetButtons(_words);
    }

    private void OnLeaderWordChanged(int index)
    {
        _wordsPanel.SetLeaderWord(index);
    }

    private void OnDrawingComplete(Texture2D texture)
    {
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        _drawnImage.sprite = screenshotSprite;

        ToggleScreen(false);

        _turnHandler.EndTurn();
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
        }

        _turnHandler.EndTurn();
    }
}
