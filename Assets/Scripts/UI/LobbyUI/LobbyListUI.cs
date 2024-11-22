using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyListUI : MenuBase
{
    public override MenuName Menu => MenuName.LobbyList;

    [Header("Lobby List Fields")]
    [SerializeField] private LobbyItemUI _lobbyItemPrefab;
    [SerializeField] private Transform _container;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _joinPrivateButton;
    [SerializeField] private Button _joinButton;

    [Header("Update List Timer")]
    [SerializeField] private float _updateEverySeconds = 10f;
    private float _updateTimer;

    private ObjectPool<LobbyItemUI> _itemPool;
    private List<LobbyItemUI> _activeItems;

    private OnlineGameManager _gameManager;

    private LocalLobby _selectedLobby;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        _itemPool = new ObjectPool<LobbyItemUI>(_lobbyItemPrefab);
        _activeItems = new List<LobbyItemUI>();

        _backButton.onClick.AddListener(OnClick_BackButton);
        _quickJoinButton.onClick.AddListener(OnClick_QuickJoinButton);
        _createButton.onClick.AddListener(OnClick_CreateButton);
        _joinPrivateButton.onClick.AddListener(OnClick_JoinPrivateButton);
        _joinButton.onClick.AddListener(OnClick_JoinButton);

        _gameManager = OnlineGameManager.Instance;
    }

    private void Start()
    {
        _gameManager.LobbyList.onLobbyListChanged += OnLobbyListChanged;

        _updateTimer = _updateEverySeconds;
    }

    private void Update()
    {
        HandleUpdate();
    }

    private void OnDestroy()
    {
        _gameManager.LobbyList.onLobbyListChanged += OnLobbyListChanged;
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
        LobbyItemUI item = _itemPool.GetObject();
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
        //MainMenuManager.Instance.ChangeMenu(MenuName.LocalOnline);

        SceneManager.UnloadSceneAsync("OnlineMenu");
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

        // MainMenuManager.Instance.ChangeMenu(MenuName.LobbyCreate);

    }

    private void OnClick_JoinButton()
    {
        if (_selectedLobby == null)
        {
            Debug.Log("You need to select lobby!");
            return;
        }

        _gameManager.JoinLobby(_selectedLobby.LobbyID.Value, _selectedLobby.LobbyCode.Value);
    }

    #endregion

}
