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

    [Header("Loading Screen Settings")]
    [SerializeField] private string _creatingLobbyLoadingText = "Creating your room...";

    private LocalLobby _localLobby;
    private OnlineController _gameManager;

    private bool _isPrivate = false;
    private int _maxPlayers = 4;

    private PlayerStatus _status;

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

        _timerSlider.onValueChanged.AddListener(OnValueChanged_TimerSlider);
        _enableTimerButton.onClick.AddListener(OnClick_EnableTimerButton);

        _gameManager.AllPlayersReadyEvent += OnPlayersReady;
        _gameManager.PlayerNotReadyEvent += OnPlayerUnready;

        _countdown.CountdownFinishedEvent += OnCountdownFinished;

        _countdown.gameObject.SetActive(true);
    }

    #region UI Events Handlers

    private async void OnClick_CreateButtonAsync()
    {
        // TODO: PRIVATE MAX PLAYERS

        LoadingPanel.Instance.Show(_creatingLobbyLoadingText);

        await _gameManager.CreateLobby(_gameManager.LocalPlayer.DisplayName.Value, _isPrivate, _maxPlayers);

        HostSetUp();

        LoadingPanel.Instance.Hide();
    }

    private void OnClick_BackButton()
    {
        _gameManager.LeaveLobby();

        _localLobby = null;

        ClearPlayersList();

        MainMenuManager.Instance.ChangeMenu(MenuName.RoomsList);
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
        _publicButtonAnimator.SetBool("Selected", true);
        _privateButtonAnimator.SetBool("Selected", false);

        _isPrivate = false;

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _enableTimerButton.gameObject.SetActive(_isPrivate);
    }

    private void OnClick_ReadyButton()
    {
        _status = PlayerStatus.Ready;

        _gameManager.SetLocalPlayerStatus(_status);

        _unreadyButton.gameObject.SetActive(true);

        _readyButton.gameObject.SetActive(false);
    }

    private void OnClick_UnreadyButton()
    {
        _status = PlayerStatus.Lobby;

        _gameManager.SetLocalPlayerStatus(_status);

        _readyButton.gameObject.SetActive(true);

        _unreadyButton.gameObject.SetActive(false);
    }

    private void OnClick_ChooseLeaderButton()
    {

    }

    private void OnClick_RandomLeaderButton()
    {
        string newLeaderID = LeaderPicker.PickRandomLeader();

        _gameManager.SetLocalLobbyLeader(newLeaderID);
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

        _chooseLeaderButton.gameObject.SetActive(false);
        _chooseLeaderButton.interactable = false;

        _randomLeaderButton.interactable = false;

        _enableTimerButton.gameObject.SetActive(false);

        _playersListUI.AddPlayer(_gameManager.LocalPlayer);

        _privateButtonAnimator.SetBool("Selected", false);
        _publicButtonAnimator.SetBool("Selected", true);

        _isPrivate = false;
    }

    private void HostSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.onUserJoined += AddPlayer;
        _localLobby.onUserLeft += RemovePlayer;

        _privateButton.interactable = false;
        _publicButton.interactable = false;

        _createButton.gameObject.SetActive(false);

        _readyButton.gameObject.SetActive(true);

        _chooseLeaderButton.gameObject.SetActive(_isPrivate);
        _chooseLeaderButton.interactable = true;

        _randomLeaderButton.interactable = _isPrivate;

        _enableTimerButton.interactable = false;

        _timerSlider.interactable = false;

        _status = _gameManager.LocalPlayer.UserStatus.Value;

        _privateButtonAnimator.SetBool("Selected", _isPrivate);
        _publicButtonAnimator.SetBool("Selected", !_isPrivate);

        UpdatePlayersList();
    }

    private void JoinSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.onUserJoined += AddPlayer;
        _localLobby.onUserLeft += RemovePlayer;

        _readyButton.gameObject.SetActive(true);
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

        _status = _gameManager.LocalPlayer.UserStatus.Value;

        UpdatePlayersList();
    }

    #endregion

    #region Players List

    private void AddPlayer(LocalPlayer player)
    {
        _playersListUI.AddPlayer(player);
    }

    private void RemovePlayer(int index)
    {
        _playersListUI.RemovePlayer(index);
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
