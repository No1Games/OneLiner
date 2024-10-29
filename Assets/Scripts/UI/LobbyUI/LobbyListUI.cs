using ModestTree;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyListUI : MenuBase
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    public override MenuName Menu => MenuName.LobbyList;

    [Header("Lobby List Fields")]
    [SerializeField] private LobbyItemUI _lobbyItemTemplate;
    [SerializeField] private Transform _container;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Button _createLobbyButton;
    [SerializeField] private Button _joinCodeButton;

    [Space]
    [SerializeField] private TMP_InputField _codeInput;

    private string _codeStr;

    private List<LobbyItemUI> _lobbyItemPool = new List<LobbyItemUI>();

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        GameManager.Instance.LobbyList.onLobbyListChanged += OnLobbyListChanged;
    }

    private void OnDestroy()
    {
        GameManager.Instance.LobbyList.onLobbyListChanged += OnLobbyListChanged;
    }

    private void UpdateLobbyList(Dictionary<string, LocalLobby> lobbies)
    {
        ClearLobbyList();

        if (lobbies.Count > _lobbyItemPool.Count)
        {
            InstantiateLobbyItems(lobbies.Count - _lobbyItemPool.Count);
        }

        List<LocalLobby> localLobbiesList = lobbies.Values.ToList();

        for (int i = 0; i < lobbies.Count; i++)
        {
            _lobbyItemPool[i].gameObject.SetActive(true);
            _lobbyItemPool[i].SetLocalLobby(localLobbiesList[i]);
        }
    }

    private void InstantiateLobbyItems(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiateLobbyItem();
        }
    }

    private void InstantiateLobbyItem()
    {
        LobbyItemUI item = Instantiate(_lobbyItemTemplate, _container);
        item.gameObject.SetActive(false);
        _lobbyItemPool.Add(item);
    }

    private void ClearLobbyList()
    {
        foreach (var item in _lobbyItemPool)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void OnLobbyListChanged(Dictionary<string, LocalLobby> lobbies)
    {
        UpdateLobbyList(lobbies);
    }

    #region Click Handlers

    private void OnClick_BackButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LocalOnline);
    }

    private void OnClick_RefreshButton()
    {
        GameManager.Instance.QueryLobbies();
    }

    private void OnClick_CreateButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LobbyCreate);
    }

    private void OnClick_JoinCodeButton()
    {
        GameManager.Instance.JoinLobby(null, _codeStr);
    }

    #endregion

    private void OnValueChanged_CodeInput(string value)
    {
        _codeStr = value;

        _joinCodeButton.interactable = !_codeStr.IsEmpty();
    }

    #region Base Methods

    public override void Init()
    {
        _lobbyItemTemplate.gameObject.SetActive(false);

        _refreshButton.onClick.AddListener(OnClick_RefreshButton);
        _createLobbyButton.onClick.AddListener(OnClick_CreateButton);
        _backButton.onClick.AddListener(OnClick_BackButton);
        _joinCodeButton.onClick.AddListener(OnClick_JoinCodeButton);

        _joinCodeButton.interactable = false;

        _codeInput.onValueChanged.AddListener(OnValueChanged_CodeInput);
    }

    #endregion
}
