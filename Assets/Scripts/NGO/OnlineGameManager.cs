using UnityEngine;
using UnityEngine.UI;

public class OnlineGameManager : MonoBehaviour
{
    #region Hearts Fields

    [Header("Hearts Fields")]
    [SerializeField] private HeartsUI _heartsUI;
    [SerializeField] private int _maxHearts = 2;
    private int _currentHearts;

    #endregion

    #region Lines Fields

    [Header("Lines")]
    [SerializeField] private LinesUI _linesUI;
    private int _linesCount = 0;

    #endregion

    #region Turn Handle Fields

    [Header("Turn Handle Fields")]
    [SerializeField]
    private StartTurnPanelUI _startTurnPanel;
    [SerializeField]
    private TurnHandler _turnHandler;

    #endregion

    #region Words Fields

    [Header("Words Fields")]
    [SerializeField]
    private WordsPanel _wordsPanel;
    private OnlineWordsManager _onlineWordsManager;

    #endregion

    #region Drawing Fields

    [Header("Drawing Fields")]
    [SerializeField] private NGODrawManager _drawingManager;
    [SerializeField] private DrawingUpdate _drawingUpdate;
    [SerializeField] private Image _drawnImage;

    #endregion

    [Space]
    [SerializeField] private Camera _mainCamera;

    [Space]
    [SerializeField] private GameOverUI _gameOverUI;

    [Space]
    [SerializeField]
    private RpcHandler _rpcHandler; // This object helds all rpc methods

    private void Awake()
    {
        _onlineWordsManager = FindFirstObjectByType<OnlineWordsManager>();
    }

    private void Start()
    {
        _currentHearts = _maxHearts;
        _heartsUI.Init(_currentHearts);

        _linesCount = 0;
        _linesUI.SetLines(_linesCount);

        SubscribeOnRpcEvents();

        _drawingManager.OnLineConfirmed += OnLineConfirmed;
        _drawingManager.OnLineSpawned += OnLineSpawned;
        _drawingManager.OnLineSpawned += UpdateLines;
        _drawingUpdate.OnScreenshotTaken += OnScreenshotTaken;

        _wordsPanel.UserClickedWordEvent += OnUserMakeGuess;
    }

    private void SubscribeOnRpcEvents()
    {
        if (_rpcHandler == null)
        {
            Debug.LogWarning("Can't subscribe on RpcEvents: Rpc Handler is null.");
            return;
        }

        _rpcHandler.GameOverEvent += ShowGameOverScreen;
        _rpcHandler.UpdateHeartsEvent += SetHearts;
        _rpcHandler.DisableWordButtonEvent += DisableButtonByIndex;
    }

    private void UnsubscribeFromRpcEvents()
    {
        if (_rpcHandler == null)
        {
            Debug.LogWarning("Can't unsubscribe from RpcEvents: Rpc Handler is null.");
            return;
        }

        _rpcHandler.GameOverEvent -= ShowGameOverScreen;
        _rpcHandler.UpdateHeartsEvent -= SetHearts;
        _rpcHandler.DisableWordButtonEvent -= DisableButtonByIndex;
    }

    private void OnDestroy()
    {
        _drawingManager.OnLineConfirmed -= OnLineConfirmed;
        _drawingManager.OnLineSpawned -= OnLineSpawned;
        _drawingManager.OnLineSpawned -= UpdateLines;
        _drawingUpdate.OnScreenshotTaken -= OnScreenshotTaken;

        UnsubscribeFromRpcEvents();
    }

    #region Words Methods

    public void DisableButtonByIndex(int index)
    {
        _wordsPanel.DisableButton(index);
    }

    #endregion

    #region Drawing Methods

    public void ToggleScreen(bool isDrawing)
    {
        _drawingUpdate.gameObject.SetActive(isDrawing);
        _mainCamera.gameObject.SetActive(!isDrawing);
        _drawingManager.IsDrawAllowed = isDrawing;
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

    private void OnLineSpawned()
    {
        ToggleScreen(true);
        _drawingUpdate.TakeScreenshot();
        ToggleScreen(false);
    }

    private void OnLineConfirmed(Line line)
    {
        _rpcHandler.SpawnLine(line.Start, line.End);
        _turnHandler.EndTurnRpc();
    }

    #endregion

    private void UpdateLines()
    {
        _linesUI.SetLines(++_linesCount);
    }

    private void OnUserMakeGuess(int index)
    {
        if (_onlineWordsManager.LeaderWordIndex.Value != index)
        {
            int newHearts = _currentHearts - 1;

            if (newHearts <= 0)
            {
                _rpcHandler.OnGameOver(false);
            }
            else
            {
                _rpcHandler.OnGuessedWrong(index, newHearts);
            }

            _turnHandler.EndTurnRpc();
        }
        else
        {
            _rpcHandler.OnGameOver(true, 100f);
        }
    }

    public void SetHearts(int count)
    {
        _currentHearts = count;

        _heartsUI.UpdateHearts(count);
    }

    public void ShowGameOverScreen(bool isWin, float score = 0)
    {
        _gameOverUI.Show(_onlineWordsManager.LeaderWord, isWin, score);
    }

}
