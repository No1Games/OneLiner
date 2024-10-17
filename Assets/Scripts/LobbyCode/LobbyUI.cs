using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : MenuBase
{
    public static LobbyUI Instance { get; private set; }

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
    [SerializeField] private Button _stratGameBtn;

    public override MenuName Menu => MenuName.Lobby;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        GameManager.Instance.JoinLobbyEvent += UpdateLobby;

        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnJoinedLobby -= UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnLeftLobby;
    }

    #region Update Lobby Methods

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(GameManager.Instance.LocalLobby);
    }

    private void UpdateLobby(LocalLobby lobby)
    {
        ClearLobby();

        if (lobby.PlayerCount > _playerItemPool.Count)
        {
            InstantiatePlayerItems(lobby.PlayerCount - _playerItemPool.Count);
        }

        for (int i = 0; i < lobby.PlayerCount; i++)
        {
            _playerItemPool[i].gameObject.SetActive(true);
            Debug.Log($"{lobby.GetLocalPlayer(i).DisplayName.Value}");
            _playerItemPool[i].SetLocalPlayer(lobby.GetLocalPlayer(i));
            _playerItemPool[i].SetKickPlayerButtonVisible(
                GameManager.Instance.LocalUser.IsHost.Value &&
                GameManager.Instance.LocalUser.ID.Value != lobby.HostID.Value);
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
            playerItem.gameObject.SetActive(false);
        }

        _codeTMP.text = "";
    }

    #endregion

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void OnClick_LeaveLobbyButton()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    #region Menu Methods

    public override void Init()
    {
        Instance = this;

        _playerItemTemplate.gameObject.SetActive(false);

        _leaveLobbyBtn.onClick.AddListener(OnClick_LeaveLobbyButton);
    }

    public override void Show()
    {
        base.Show();

        _localLobby = GameManager.Instance.LocalLobby;
    }

    #endregion
}
