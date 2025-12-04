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
    private Dictionary<string, RoomItemUI> _itemsById;

    private OnlineController _gameManager;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        // Subscribe on lobby list update
        _gameManager.LobbyManager.Lobbies.OnLobbiesUpdated += OnLobbiesUpdated;
    }

    private void Update()
    {
        HandleUpdate();
    }

    private void OnDestroy()
    {
        _gameManager.LobbyManager.Lobbies.OnLobbiesUpdated -= OnLobbiesUpdated;
    }

    #region List Update Methods

    private void HandleUpdate()
    {
        _updateTimer -= Time.deltaTime;

        if (_updateTimer <= 0)
        {
            _updateTimer = _updateEverySeconds;
            _gameManager.QueryLobbiesAsync(); // Send request and then recive an event of updated lobbies
        }
    }

    private void UpdateLobbyList(Dictionary<string, LocalLobby> lobbies)
    {
        var seen = new HashSet<string>();

        int siblingIndex = 0;

        foreach (var kvp in lobbies)
        {
            var lobby = kvp.Value;
            RoomItemUI item;

            if (_itemsById.TryGetValue(kvp.Key, out item))
            {
                // Item is already exists - update lobby
                item.SetLocalLobby(lobby);
            }
            else
            {
                // New lobby - create new item
                item = _itemPool.GetObject();
                item.transform.SetParent(_container, false);
                item.SetLocalLobby(lobby);
                item.LobbySelectedEvent += OnLobbySelected;
                item.JoinLobbyEvent += OnJoinLobbyClicked;
                _itemsById.Add(lobby.LobbyId.Value, item);
            }

            // Set order
            item.transform.SetSiblingIndex(siblingIndex++);
            // Save seen lobby
            seen.Add(kvp.Key);
        }

        // Remove closed lobbies
        var toRemove = new List<string>();
        foreach (var kvp in _itemsById)
        {
            if (!seen.Contains(kvp.Key))
            {
                var item = kvp.Value;
                item.LobbySelectedEvent -= OnLobbySelected;
                item.JoinLobbyEvent -= OnJoinLobbyClicked;
                _itemPool.ReturnObject(item);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
            _itemsById.Remove(key);
    }

    #endregion

    #region Events Handlers

    private void OnLobbiesUpdated(Dictionary<string, LocalLobby> lobbies)
    {
        UpdateLobbyList(lobbies);
    }

    private void OnLobbySelected(LocalLobby lobby)
    {
        foreach (var item in _itemsById.Values)
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

        await _gameManager.JoinLobbyByIdAsync(lobby.LobbyId.Value);

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
        Debug.LogWarning("Quick join not implemented");
    }

    private void OnClick_JoinPrivateButton()
    {
        Debug.LogWarning("Private join not implemented");
    }

    private void OnClick_CreateButton()
    {
        MainMenuManager.Instance.OpenRoomPanel(true);
    }

    #endregion

    private void DeselectAll()
    {
        if (_itemsById == null || _itemsById.Count == 0)
            return;

        foreach (var item in _itemsById.Values)
            item.Deselect();
    }

    #region Menu Methods

    public override void Init()
    {
        base.Init();

        // Init pool for items in list and list for active room items
        _itemPool = new ObjectPool<RoomItemUI>(_roomItemPrefab, _container);
        _itemsById = new Dictionary<string, RoomItemUI>();

        // Set button click listeners
        _backButton.onClick.AddListener(OnClick_BackButton);
        _quickJoinButton.onClick.AddListener(OnClick_QuickJoinButton);
        _createButton.onClick.AddListener(OnClick_CreateButton);
        _joinPrivateButton.onClick.AddListener(OnClick_JoinPrivateButton);

        // Cache online controller
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
