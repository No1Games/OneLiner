using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : MenuBase
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    private LocalLobby _localLobby;

    [Header("Players List Fields")]
    [SerializeField] private LobbyPlayerItemUI _playerItemTemplate;
    [SerializeField] private Transform _container;

    private List<LobbyPlayerItemUI> _playerItemPool = new List<LobbyPlayerItemUI>();

    [Header("Lobby Info Fields")]
    [SerializeField] private TextMeshProUGUI _lobbyNameTMP;
    [SerializeField] private TextMeshProUGUI _playersTMP;
    [SerializeField] private TextMeshProUGUI _codeTMP;

    [Header("Buttons")]
    [SerializeField] private Button _leaveLobbyBtn;
    [SerializeField] private Button _startBtn;
    [SerializeField] private Button _readyGameBtn;

    private PlayerStatus _playerReady = PlayerStatus.Lobby;

    public override MenuName Menu => MenuName.Lobby;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        _localLobby = GameManager.Instance.LocalLobby;
        _localLobby.onLobbyDataChanged += UpdateUI;

        //_startBtn.gameObject.SetActive(GameManager.Instance.LocalUser.IsHost.Value);

        _playerReady = GameManager.Instance.LocalUser.UserStatus.Value;

        UpdateUI();
    }

    #region Update UI Methods

    private void UpdateUI()
    {
        ClearLobby();

        _lobbyNameTMP.text = _localLobby.LobbyName.Value;
        _playersTMP.text = $"{_localLobby.PlayerCount} / {_localLobby.MaxPlayerCount.Value}";

        _codeTMP.text = $"{_localLobby.LobbyCode.Value}";

        UpdatePlayersList();
    }

    private void UpdatePlayersList()
    {
        if (_localLobby.PlayerCount > _playerItemPool.Count)
        {
            InstantiatePlayerItems(_localLobby.PlayerCount - _playerItemPool.Count);
        }

        for (int i = 0; i < _localLobby.PlayerCount; i++)
        {
            _playerItemPool[i].gameObject.SetActive(true);
            _playerItemPool[i].SetLocalPlayer(_localLobby.GetLocalPlayer(i));
            _playerItemPool[i].SetKickPlayerButtonVisible(
                GameManager.Instance.LocalUser.IsHost.Value &&
                _localLobby.GetLocalPlayer(i).ID.Value != _localLobby.HostID.Value);
        }
    }

    private void InstantiatePlayerItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiatePlayerItem();
        }
    }

    private void InstantiatePlayerItem()
    {
        LobbyPlayerItemUI playerItemTransform = Instantiate(_playerItemTemplate, _container);
        _playerItemPool.Add(playerItemTransform);
    }

    private void ClearLobby()
    {
        foreach (var playerItem in _playerItemPool)
        {
            playerItem.ResetUI();
        }

        _codeTMP.text = "";
    }

    #endregion

    private void OnClick_LeaveLobbyButton()
    {
        GameManager.Instance.LeaveLobby();
        MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
    }

    private void OnClick_StartButton()
    {
        GameManager.Instance.StartOnlineGame();
    }

    private void OnClick_ReadyButton()
    {
        _playerReady = _playerReady == PlayerStatus.Lobby ? PlayerStatus.Ready : PlayerStatus.Lobby;

        _logger.Log($"Ready Button Clicked: Switch status of {GameManager.Instance.LocalUser.DisplayName.Value} to {_playerReady} \n ID: {GameManager.Instance.LocalUser.ID.Value}");

        GameManager.Instance.SetLocalUserStatus(_playerReady);
        _readyGameBtn.GetComponentInChildren<TextMeshProUGUI>().text = _playerReady == PlayerStatus.Ready ? "Cancel" : "Ready";
    }

    #region Menu Methods

    public override void Init()
    {
        _playerItemTemplate.gameObject.SetActive(false);

        _leaveLobbyBtn.onClick.AddListener(OnClick_LeaveLobbyButton);
        _startBtn.onClick.AddListener(OnClick_StartButton);
        _readyGameBtn.onClick.AddListener(OnClick_ReadyButton);

        // _playerReady = GameManager.Instance.LocalUser.UserStatus.Value;
    }

    public override void Show()
    {
        base.Show();

        _localLobby = GameManager.Instance.LocalLobby;
    }

    #endregion
}
