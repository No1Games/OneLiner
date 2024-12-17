using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomsListUI : MenuBase
{
    public override MenuName Menu => MenuName.RoomsList;

    [Header("Loading Settings")]
    [SerializeField] private string _joiningText = "Joining the room...";

    [Header("Rooms List Fields")]
    [SerializeField] private RoomItemUI _roomItemPrefab;
    [SerializeField] private Transform _container;
    [SerializeField] private Image _separatorImage;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinPrivateButton;
    [SerializeField] private Button _joinButton;

    [Header("Update List Timer")]
    [SerializeField] private float _updateEverySeconds = 10f;
    private float _updateTimer;

    private ObjectPool<RoomItemUI> _itemPool;
    private List<RoomItemUI> _activeItems;

    private OnlineController _gameManager;

    private LocalLobby _selectedLobby;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        _gameManager.LobbyList.onLobbyListChanged += OnLobbyListChanged;
    }

    private void Update()
    {
        HandleUpdate();
    }

    private void OnDestroy()
    {
        _gameManager.LobbyList.onLobbyListChanged -= OnLobbyListChanged;
    }

    #region List Update Methods

    private void HandleUpdate()
    {
        _updateTimer -= Time.deltaTime;

        if (_updateTimer <= 0)
        {
            _updateTimer = _updateEverySeconds;
            _gameManager.QueryLobbies();
        }
    }

    private void UpdateLobbyList(Dictionary<string, LocalLobby> lobbies)
    {
        ClearLobbyList();

        foreach (var lobby in lobbies)
        {
            AddLobbyItem(lobby.Value);
        }
    }

    private void AddLobbyItem(LocalLobby lobby)
    {
        RoomItemUI item = _itemPool.GetObject();
        item.gameObject.transform.SetParent(_container, false);
        item.SetLocalLobby(lobby);
        item.LobbySelectedEvent += OnLobbySelected;

        _activeItems.Add(item);
    }

    private void ClearLobbyList()
    {
        foreach (var item in _activeItems)
        {
            _itemPool.ReturnObject(item);
        }

        _activeItems.Clear();
    }

    #endregion

    #region Events Handlers

    private void OnLobbyListChanged(Dictionary<string, LocalLobby> lobbies)
    {
        UpdateLobbyList(lobbies);
    }

    private void OnLobbySelected(LocalLobby lobby)
    {
        _selectedLobby = lobby;
        _joinButton.gameObject.SetActive(true);
    }

    #endregion

    #region Click Handlers

    private void OnClick_BackButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LocalOnline);
    }

    private void OnClick_QuickJoinButton()
    {

    }

    private void OnClick_JoinPrivateButton()
    {

    }

    private void OnClick_CreateButton()
    {
        OnlineMenuManager.Instance.OpenRoomPanel(true);
    }

    private async void OnClick_JoinButtonAsync()
    {
        if (_selectedLobby == null)
        {
            Debug.LogWarning("No lobby selected!");
            return;
        }

        LoadingPanel.Instance.Show(_joiningText);

        await _gameManager.JoinLobby(_selectedLobby.LobbyID.Value, _selectedLobby.LobbyCode.Value);

        OnlineMenuManager.Instance.OpenRoomPanel(false);

        LoadingPanel.Instance.Hide();
    }

    #endregion

    #region Menu Methods

    public override void Init()
    {
        base.Init();

        _itemPool = new ObjectPool<RoomItemUI>(_roomItemPrefab);
        _activeItems = new List<RoomItemUI>();

        _backButton.onClick.AddListener(OnClick_BackButton);
        _quickJoinButton.onClick.AddListener(OnClick_QuickJoinButton);
        _createButton.onClick.AddListener(OnClick_CreateButton);
        _joinPrivateButton.onClick.AddListener(OnClick_JoinPrivateButton);
        _joinButton.onClick.AddListener(OnClick_JoinButtonAsync);

        _gameManager = OnlineController.Instance;
    }

    public override void Show()
    {
        base.Show();

        _updateTimer = _updateEverySeconds;
    }

    public override void Hide()
    {
        base.Hide();
    }

    #endregion

}
