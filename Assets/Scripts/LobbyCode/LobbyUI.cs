using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    [Header("Players List Fields")]
    [SerializeField] private Transform _playerItemTemplate;
    [SerializeField] private Transform _container;

    [Header("Lobby Info Fields")]
    [SerializeField] private TextMeshProUGUI _lobbyNameTMP;
    [SerializeField] private TextMeshProUGUI _playersTMP;

    [Header("Buttons")]
    [SerializeField] private Button _leaveLobbyBtn;
    [SerializeField] private Button _stratGameBtn;

    private void Awake()
    {
        Instance = this;

        _playerItemTemplate.gameObject.SetActive(false);

        _leaveLobbyBtn.onClick.AddListener(OnClick_LeaveLobbyButton);
    }

    private void Start()
    {
        _logger.Log("LobbyUI Start");
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        Hide();
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
        UpdateLobby(LobbyManager.Instance.JoinedLobby);
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        Show();

        foreach (Player player in lobby.Players)
        {
            Transform playerItemTransform = Instantiate(_playerItemTemplate, _container);
            playerItemTransform.gameObject.SetActive(true);
            LobbyPlayerItemUI lobbyPlayerSingleUI = playerItemTransform.GetComponent<LobbyPlayerItemUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        _lobbyNameTMP.text = lobby.Name;
        _playersTMP.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
    }

    private void ClearLobby()
    {
        foreach (Transform child in _container)
        {
            if (child == _playerItemTemplate) continue;
            Destroy(child.gameObject);
        }
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

    #region Show/Hide Methods

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
