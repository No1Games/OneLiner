using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyListUI : MenuBase
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    public static LobbyListUI Instance { get; private set; }

    public override MenuName Menu => MenuName.LobbyList;

    [Header("Lobby List Fields")]
    [SerializeField] private Transform _lobbyItemTemplate;
    [SerializeField] private Transform _container;

    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Button _createLobbyButton;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
    }

    #region Events Listeners

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    #endregion

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in _container)
        {
            if (child == _lobbyItemTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            _logger.Log($"Lobby: {lobby.Name}");
            Transform lobbySingleTransform = Instantiate(_lobbyItemTemplate, _container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyItemUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyItemUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    #region Button Click

    private void OnClick_BackButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LocalOnline);
    }

    private void OnClick_RefreshButton()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void OnClick_CreateButton()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LobbyCreate);
    }

    #endregion

    #region Show/Hide Methods

    public override void Init()
    {
        Instance = this;

        _lobbyItemTemplate.gameObject.SetActive(false);

        _refreshButton.onClick.AddListener(OnClick_RefreshButton);
        _createLobbyButton.onClick.AddListener(OnClick_CreateButton);
        _backButton.onClick.AddListener(OnClick_BackButton);
    }

    #endregion
}
