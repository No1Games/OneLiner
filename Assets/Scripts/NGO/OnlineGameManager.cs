using System;
using System.Collections.Generic;
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
    [SerializeField] private StartTurnPanelUI _startTurnPanel;
    [SerializeField] private TurnHandler _turnHandler;

    #endregion

    #region Words Fields

    [Header("Words Fields")]
    [SerializeField] private WordsPanel _wordsPanel;
    [SerializeField] private WordManager _wordManager;
    private List<string> _wordsList = new List<string>();
    //private NetworkVariable<List<int>> _wordsIndexes = new NetworkVariable<List<int>>();
    private string _leaderWord;
    private int _leaderWordIndex;

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

    // Fields for chaching controller and local models
    private OnlineController _onlineController;
    private LocalLobby _localLobby;
    private LocalPlayer _localPlayer;

    private void Awake()
    {
        // Chach controller and models
        _onlineController = OnlineController.Instance;
        _localLobby = _onlineController.LocalLobby;
        _localPlayer = _onlineController.LocalPlayer;
    }

    private void Start()
    {
        _currentHearts = _maxHearts;
        _heartsUI.Init(_currentHearts);

        _linesCount = 0;
        _linesUI.SetLines(_linesCount);

        SubscribeOnRpcEvents();

        //_localLobby.WordsList.onChanged += OnWordsChanged;
        //_localLobby.LeaderWord.onChanged += OnLeaderWordChanged;

        // Host generates words
        // TODO: Sync via RPC
        if (_localPlayer.IsHost.Value)
        {
            //SetWords();
            //_wordsList = _wordManager.FormWordListForRound();
            //_wordsIndexes = _wordManager.GetWordsIndexes(_wordsList);

            //// _rpcHandler.SetWordsIndexes();

            //_leaderWordIndex = _wordManager.GetLeaderWordIndex(_wordsList);
            //_leaderWord = _wordsList[_leaderWordIndex];
            //_onlineController.LobbyManager.LocalLobbyEditor
            //    .SetWordsIndexes(_wordsIndexes)
            //    .SetLeaderWordIndex(_leaderWordIndex)
            //    .CommitChangesAsync();
        }

        _drawingManager.OnLineConfirmed += OnLineConfirmed;
        _drawingManager.OnLineSpawned += OnLineSpawned;
        _drawingManager.OnLineSpawned += UpdateLines;
        _drawingUpdate.OnScreenshotTaken += OnScreenshotTaken;

        _wordsPanel.UserClickedWordEvent += OnUserMakeGuess;
    }

    //public override void OnNetworkSpawn()
    //{
    //    _wordsIndexes.OnValueChanged += OnWordsChanged;

    //    if (_localPlayer.IsHost.Value)
    //    {
    //        SetWordsRpc();
    //    }
    //}

    //public override void OnNetworkDespawn()
    //{
    //    _wordsIndexes.OnValueChanged -= OnWordsChanged;
    //}

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
        _localLobby.WordsList.onChanged -= OnWordsChanged;
        _localLobby.LeaderWord.onChanged -= OnLeaderWordChanged;

        _drawingManager.OnLineConfirmed -= OnLineConfirmed;
        _drawingManager.OnLineSpawned -= OnLineSpawned;
        _drawingManager.OnLineSpawned -= UpdateLines;
        _drawingUpdate.OnScreenshotTaken -= OnScreenshotTaken;

        UnsubscribeFromRpcEvents();
    }

    #region Words Methods

    //[Rpc(SendTo.NotServer)]
    //private void SetWordsRpc()
    //{
    //    _wordsList = _wordManager.FormWordListForRound();
    //    _wordsIndexes.Value = _wordManager.GetWordsIndexes(_wordsList);
    //    _wordsIndexes.CheckDirtyState();
    //}

    private void OnWordsChanged(List<int> previous, List<int> current)
    {
        _wordsList = _wordManager.GetWordsFromIndexes(current);
        _wordsPanel.SetButtons(_wordsList);
    }

    private void OnWordsChanged(List<int> indexes)
    {
        _wordsList = _wordManager.GetWordsFromIndexes(indexes);

        _wordsPanel.SetButtons(_wordsList);
    }

    private void OnLeaderWordChanged(int index)
    {
        _leaderWordIndex = index;

        if (_wordsList != null)
        {
            try
            {
                // m_LeaderWord = m_WordsList[m_LeaderWordIndex];
                Debug.LogWarning(_localLobby.WordsList.Value.Count);
                Debug.LogWarning(_wordManager.GetWordsFromIndexes(_localLobby.WordsList.Value).Count);
                _wordsList = _wordManager.GetWordsFromIndexes(_localLobby.WordsList.Value);
                _leaderWord = _wordsList[index];
            }
            catch (Exception e)
            {
                Debug.LogError($"{_wordsList.Count} {_leaderWordIndex}\n{e.Message}");
                _leaderWord = null;
            }
        }

        _wordsPanel.SetLeaderWord(index);
    }

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

    private async void OnLineConfirmed(Line line)
    {
        _rpcHandler.SpawnLine(line.Start, line.End);
        await _turnHandler.EndTurn();
    }

    #endregion

    private void UpdateLines()
    {
        _linesUI.SetLines(++_linesCount);
    }

    private async void OnUserMakeGuess(int index)
    {
        if (_leaderWordIndex != index)
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

            await _turnHandler.EndTurn();
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
        if (_leaderWord == null)
        {
            if (_wordsList != null)
            {
                _leaderWord = _wordsList[_leaderWordIndex];
            }
            else
            {
                Debug.LogError($"Words list was not initialilzed!");
            }
        }

        _gameOverUI.Show(_leaderWord, isWin, score);
    }

}
