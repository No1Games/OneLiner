using System;
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

        SubscribeOnRpc();

        _drawingManager.OnLineConfirmed += OnLineConfirmed;
        _drawingManager.OnLineSpawned += OnLineSpawned;
        _drawingManager.OnLineSpawned += UpdateLines;
        _drawingUpdate.OnScreenshotTaken += OnScreenshotTaken;

        _wordsPanel.Init(OnUserMakeGuess);
    }

    private void SubscribeOnRpc()
    {
        if (_rpcHandler == null)
        {
            Debug.LogWarning("Can't subscribe on RpcEvents: Rpc Handler is null.");
            return;
        }

        _rpcHandler.OnRpcEvent += HandleRpcEvent;
    }

    private void UnsubscribeFromRpc()
    {
        if (_rpcHandler == null)
        {
            Debug.LogWarning("Can't unsubscribe from RpcEvents: Rpc Handler is null.");
            return;
        }

        _rpcHandler.OnRpcEvent -= HandleRpcEvent;
    }

    private void OnDestroy()
    {
        _drawingManager.OnLineConfirmed -= OnLineConfirmed;
        _drawingManager.OnLineSpawned -= OnLineSpawned;
        _drawingManager.OnLineSpawned -= UpdateLines;
        _drawingUpdate.OnScreenshotTaken -= OnScreenshotTaken;

        UnsubscribeFromRpc();
    }

    private void HandleRpcEvent(RpcEvent data)
    {
        switch (data.Type)
        {
            case RpcEventType.UserGuess: HandleUserGuess((int)data.Payload); break;
            case RpcEventType.WordDisabled: HandleWordDisabled((int)data.Payload); break;
            case RpcEventType.HeartsUpdated: HandleHeartsUpdated((int)data.Payload); break;
            case RpcEventType.LineSpawned: HandleLineSpawned(((Vector3, Vector3))data.Payload); break;
            case RpcEventType.GameOver: HandleGameOver(((bool, float))data.Payload); break;
        }
    }

    // This method runs only on server
    private void HandleUserGuess(int index)
    {
        if (_onlineWordsManager.LeaderWordIndex.Value != index)
        {
            int newHearts = _currentHearts - 1;

            if (newHearts <= 0)
                _rpcHandler.OnGameOver(false);
            else
                _rpcHandler.OnWrongGuess(index, newHearts);

            _turnHandler.EndTurnRpc();
        }
        else
            _rpcHandler.OnGameOver(true, 100f);
    }

    private void HandleWordDisabled(int index)
    {
        _wordsPanel.DisableButton(index);
    }

    private void HandleHeartsUpdated(int payload)
    {
        throw new NotImplementedException();
    }

    // When event received
    //  1) spawn line (if not spawned already)
    //  2) take screenshot
    //  3) change image
    private void HandleLineSpawned((Vector3, Vector3) payload)
    {
        _drawingManager.SpawnLine(payload.Item1, payload.Item2); // spawn line, TODO: do not duplicate line
        OnLineSpawned(); // Take screenshot
        // Take Screenshot invokes the event and OnScreenshotTaken changes image
    }

    private void HandleGameOver((bool, float) payload)
    {
        throw new NotImplementedException();
    }

    private void OnUserMakeGuess(int index)
    {
        var controller = OnlineController.Instance;
        var player = controller.LocalPlayer;
        var lobby = controller.LocalLobby;

        if (lobby.LeaderID.Value == player.PlayerId.Value)
        {
            Debug.Log("Leader can't guess words!");
            return;
        }

        if (_turnHandler.CurrentPlayerId.Value.ToString() != player.PlayerId.Value)
        {
            Debug.Log("It's not your turn!");
            return;
        }

        _wordsPanel.Hide();

        _rpcHandler.OnUserGuess(index); // Send index to server. Server decides what happens next
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
        Debug.Log("Update Picture");

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

    // When user confirmed line - send event to all players and end turn
    private void OnLineConfirmed(Vector3 start, Vector3 end)
    {
        _rpcHandler.OnSpawnLine(start, end); // Send line to other players
        _turnHandler.EndTurnRpc(); // End turn
    }

    #endregion

    private void UpdateLines()
    {
        _linesUI.SetLines(++_linesCount);
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
