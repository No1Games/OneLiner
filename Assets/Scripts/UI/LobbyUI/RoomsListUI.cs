using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomsListUI : MenuBase
{
    public override MenuName Menu => MenuName.RoomsList;

    [Header("Rooms List Fields")]
    [SerializeField] private RoomItemUI _roomItemPrefab;
    [SerializeField] private Transform _container;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinPrivateButton;

    [Header("Update List Timer")]
    [SerializeField] private float _updateEverySeconds = 4f;
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
        _gameManager.LobbyManager.Lobbies.OnLobbiesUpdated += OnLobbiesUpdated;
        //_gameManager.LobbyList.onLobbyListChanged += OnLobbyListChanged;
    }

    private void Update()
    {
        HandleUpdate();
    }

    private void OnDestroy()
    {
        _gameManager.LobbyManager.Lobbies.OnLobbiesUpdated -= OnLobbiesUpdated;
        // _gameManager.LobbyList.onLobbyListChanged -= OnLobbiesUpdated;
    }

    #region List Update Methods

    private void HandleUpdate()
    {
        _updateTimer -= Time.deltaTime;

        if (_updateTimer <= 0)
        {
            _updateTimer = _updateEverySeconds;
            _gameManager.QueryLobbiesAsync();
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
        item.JoinLobbyEvent += OnJoinLobbyClicked;

        _activeItems.Add(item);
    }

    private void ClearLobbyList()
    {
        foreach (var item in _activeItems)
        {
            item.LobbySelectedEvent -= OnLobbySelected;
            item.JoinLobbyEvent -= OnJoinLobbyClicked;
            _itemPool.ReturnObject(item);
        }

        _activeItems.Clear();
    }

    #endregion

    #region Events Handlers

    private void OnLobbiesUpdated(Dictionary<string, LocalLobby> lobbies)
    {
        UpdateLobbyList(lobbies);
    }

    private void OnLobbySelected(LocalLobby lobby)
    {
        foreach (var item in _activeItems)
        {
            if (item.LocalLobby != lobby)
            {
                item.Deselect();
            }
        }
    }

    private async void OnJoinLobbyClicked(LocalLobby lobby)
    {
        LoadingPanel.Instance.Show();

        await _gameManager.JoinLobbyByIdAsync(lobby.LobbyID.Value);
        //await _gameManager.JoinLobby(lobby.LobbyID.Value, lobby.LobbyCode.Value);

        MainMenuManager.Instance.OpenRoomPanel(false);

        LoadingPanel.Instance.Hide();
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
        MainMenuManager.Instance.OpenRoomPanel(true);
    }

    #endregion

    private void DeselectAll()
    {
        if (_activeItems == null)
        {
            Debug.LogWarning("RoomsList active items is null");
            return;
        }

        foreach (var item in _activeItems)
        {
            item.Deselect();
        }
    }

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

        _gameManager = OnlineController.Instance;
    }

    public override void Show()
    {
        base.Show();

        _updateTimer = _updateEverySeconds;
    }

    public override void Hide()
    {
        DeselectAll();

        base.Hide();
    }

    #endregion

}
