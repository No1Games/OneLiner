using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MenuBase
{
    public override MenuName Menu => MenuName.WaitingRoom;

    [Header("Is Private UI")]
    [SerializeField] private Button _privateButton;
    [SerializeField] private Button _publicButton;
    [SerializeField] private Animator _privateButtonAnimator;
    [SerializeField] private Animator _publicButtonAnimator;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _createButton;

    [Header("Ready Menu")]
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _unreadyButton;
    [SerializeField] private Countdown _countdown;

    [Space]
    [SerializeField] private PlayersListUI _playersListUI;

    [Header("Leader Settings")]
    [SerializeField] private Button _chooseLeaderButton;
    [SerializeField] private Button _randomLeaderButton;

    [Header("Timer Settings")]
    [SerializeField] private Button _enableTimerButton;
    [SerializeField] private TextMeshProUGUI _timeTMP;
    [SerializeField] private Slider _timerSlider;
    [SerializeField] private Image _timerEnabledImage;
    private float _timerValue;
    private bool _isTimerEnabled;

    private LocalLobby _localLobby;
    private OnlineController _gameManager;

    private bool _isPrivate = false;
    private int _maxPlayers = 4;

    private void Awake()
    {
        _gameManager = OnlineController.Instance;

        _backButton.onClick.AddListener(OnClick_BackButton);

        _privateButton.onClick.AddListener(OnClick_PrivateButton);
        _publicButton.onClick.AddListener(OnClick_PublicButton);

        _createButton.onClick.AddListener(OnClick_CreateButtonAsync);
        _readyButton.onClick.AddListener(OnClick_ReadyButton);
        _unreadyButton.onClick.AddListener(OnClick_UnreadyButton);

        _randomLeaderButton.onClick.AddListener(OnClick_RandomLeaderButton);
        _chooseLeaderButton.onClick.AddListener(OnClick_ChooseLeaderButton);

        _timerSlider.onValueChanged.AddListener(OnValueChanged_TimerSlider);
        _enableTimerButton.onClick.AddListener(OnClick_EnableTimerButton);

        _gameManager.AllPlayersReadyEvent += OnPlayersReady;
        _gameManager.PlayerNotReadyEvent += OnPlayerUnready;

        _countdown.CountdownFinishedEvent += OnCountdownFinished;

        _countdown.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _gameManager.AllPlayersReadyEvent -= OnPlayersReady;
        _gameManager.PlayerNotReadyEvent -= OnPlayerUnready;

        _countdown.CountdownFinishedEvent -= OnCountdownFinished;

        _localLobby.PlayerJoined -= AddPlayer;
        _localLobby.PlayerRemoved -= RemovePlayer;
    }

    #region UI Events Handlers

    private async void OnClick_CreateButtonAsync()
    {
        // TODO: PRIVATE MAX PLAYERS

        LoadingPanel.Instance.Show();

        await _gameManager.CreateLobbyAsync(_isPrivate, _maxPlayers);

        HostSetUp();

        LoadingPanel.Instance.Hide();
    }

    private async void OnClick_BackButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.RoomsList);

        await _gameManager.LeaveLobbyAsync();

        _localLobby = null;

        ClearPlayersList();
    }

    private void OnClick_PrivateButton()
    {
        // TODO: Check for PREM

        _privateButtonAnimator.SetBool("Selected", true);
        _publicButtonAnimator.SetBool("Selected", false);

        _isPrivate = true;

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _enableTimerButton.gameObject.SetActive(_isPrivate);
    }

    private void OnClick_PublicButton()
    {
        OnlineController.Instance.IsRandomLeader = true;

        _publicButtonAnimator.SetBool("Selected", true);
        _privateButtonAnimator.SetBool("Selected", false);

        _isPrivate = false;

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _enableTimerButton.gameObject.SetActive(_isPrivate);
    }

    private async void OnClick_ReadyButton()
    {
        // Update UI
        _readyButton.gameObject.SetActive(false); // Off Ready
        _unreadyButton.gameObject.SetActive(true); // On Unready

        // Change player status locally and commit changes to Lobby Service
        await _gameManager.LobbyManager.LocalPlayerEditor
            .SetStatus(PlayerStatus.Ready)
            .CommitChangesAsync();
    }

    private async void OnClick_UnreadyButton()
    {
        // Update UI
        _unreadyButton.gameObject.SetActive(false); // Off Unready
        _readyButton.gameObject.SetActive(true); // On Ready

        // Change player status locally and commit changes to Lobby Service
        await _gameManager.LobbyManager.LocalPlayerEditor
            .SetStatus(PlayerStatus.Lobby)
            .CommitChangesAsync();
    }

    private void OnClick_ChooseLeaderButton()
    {
        OnlineController.Instance.IsRandomLeader = false;
    }

    private void OnClick_RandomLeaderButton()
    {

    }

    private void OnClick_EnableTimerButton()
    {
        _isTimerEnabled = !_isTimerEnabled;
        _timerEnabledImage.gameObject.SetActive(_isTimerEnabled);
        _timerSlider.interactable = _isTimerEnabled;
    }

    private void OnValueChanged_TimerSlider(float value)
    {
        _timerValue = value;
        _timeTMP.text = _timerValue.ToString();
    }

    #endregion

    private void OnPlayersReady()
    {
        _unreadyButton.gameObject.SetActive(false);

        _countdown.gameObject.SetActive(true);
        _countdown.StartCountDown();
    }

    private void OnPlayerUnready()
    {
        if (_gameManager.LocalPlayer.Status.Value != PlayerStatus.Ready) return;

        _unreadyButton.gameObject.SetActive(true);

        _countdown.CancelCountDown();
        _countdown.gameObject.SetActive(false);
    }

    private void OnCountdownFinished()
    {
        _gameManager.OnCountdownFinished();
    }

    public void Show(bool isCreate)
    {
        gameObject.SetActive(true);

        _gameManager = OnlineController.Instance;

        if (isCreate)
        {
            CreateSetUp();
        }
        else
        {
            JoinSetUp();
        }
    }

    #region Set Ups

    private void CreateSetUp()
    {
        _createButton.gameObject.SetActive(true);

        _readyButton.gameObject.SetActive(false);
        _unreadyButton.gameObject.SetActive(false);

        _chooseLeaderButton.gameObject.SetActive(false);
        _chooseLeaderButton.interactable = false;

        _randomLeaderButton.interactable = false;

        _enableTimerButton.gameObject.SetActive(false);

        _privateButtonAnimator.SetBool("Selected", false);
        _publicButtonAnimator.SetBool("Selected", true);

        _isPrivate = false;

        _playersListUI.AddPlayer(_gameManager.LocalPlayer);
    }

    private void HostSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.PlayerJoined += AddPlayer;
        _localLobby.PlayerRemoved += RemovePlayer;

        _privateButton.interactable = false;
        _publicButton.interactable = false;

        _createButton.gameObject.SetActive(false);

        _readyButton.gameObject.SetActive(true);
        _unreadyButton.gameObject.SetActive(false);

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _chooseLeaderButton.interactable = true;

        _randomLeaderButton.interactable = _isPrivate;

        _enableTimerButton.interactable = false;

        _timerSlider.interactable = false;

        _privateButtonAnimator.SetBool("Selected", _isPrivate);
        _publicButtonAnimator.SetBool("Selected", !_isPrivate);

        UpdatePlayersList();
    }

    private void JoinSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.PlayerJoined += AddPlayer;
        _localLobby.PlayerRemoved += RemovePlayer;

        _readyButton.gameObject.SetActive(true);
        _unreadyButton.gameObject.SetActive(false);
        _createButton.gameObject.SetActive(false);

        _privateButton.interactable = false;
        _publicButton.interactable = false;

        _isPrivate = _localLobby.Private.Value;

        _privateButtonAnimator.SetBool("Selected", _isPrivate);
        _publicButtonAnimator.SetBool("Selected", !_isPrivate);

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _chooseLeaderButton.interactable = false;

        _randomLeaderButton.interactable = false;

        _enableTimerButton.interactable = false;

        _timerSlider.interactable = false;
        _enableTimerButton.gameObject.SetActive(false);

        UpdatePlayersList();
    }

    #endregion

    #region Players List

    private void AddPlayer(LocalPlayer player)
    {
        Debug.Log("Adding player");
        _playersListUI.AddPlayer(player);
    }

    private void RemovePlayer(LocalPlayer player)
    {
        _playersListUI.RemovePlayer(player);
    }

    private void UpdatePlayersList()
    {
        _playersListUI.UpdateList(_localLobby.LocalPlayers);
    }

    private void ClearPlayersList()
    {
        _playersListUI.ClearList();
    }

    #endregion
}
