using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour
{
    [SerializeField] private Button _isPrivateButton;
    [SerializeField] private TextMeshProUGUI _isPrivateTMP;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _readyButton;

    [SerializeField] private PlayersListUI _playersListUI;

    private LocalLobby _localLobby;
    private OnlineGameManager _gameManager;

    private bool _isPrivate = false;
    private string _lobbyName = "";
    private int _maxPlayers = 4;

    private void Awake()
    {
        _gameManager = OnlineGameManager.Instance;

        _backButton.onClick.AddListener(OnClick_BackButton);
        _isPrivateButton.onClick.AddListener(OnClick_IsPrivateButton);
        _createButton.onClick.AddListener(OnClick_CreateButton);
    }

    #region Click Handlers

    private void OnClick_CreateButton()
    {
        // TODO: PRIVATE MAX PLAYERS

        _gameManager.CreateLobby(_gameManager.LocalUser.DisplayName.Value, _isPrivate, _maxPlayers);

        HostSetUp();
    }

    private void OnClick_BackButton()
    {
        _gameManager.LeaveLobby();

        _localLobby = null;

        ClearPlayersList();

        OnlineMenuManager.Instance.OpenLobbyList();
    }

    private void OnClick_IsPrivateButton()
    {
        // TODO: CHECK FOR PREMIUM

        _isPrivate = !_isPrivate;

        _isPrivateTMP.text = _isPrivate ? "PRIVATE" : "PUBLIC";
    }

    #endregion

    public void Show(bool isCreate)
    {
        gameObject.SetActive(true);

        _gameManager = OnlineGameManager.Instance;

        if (isCreate)
        {
            CreateSetUp();
        }
        else
        {
            JoinSetUp();
        }
    }

    private void CreateSetUp()
    {
        _lobbyName = _gameManager.LocalUser.DisplayName.Value;

        _createButton.gameObject.SetActive(true);

        _isPrivateButton.gameObject.SetActive(true);

        _readyButton.gameObject.SetActive(false);

        _playersListUI.AddPlayer(_gameManager.LocalUser);
    }

    private void HostSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.onUserJoined += AddPlayer;
        _localLobby.onUserLeft += RemovePlayer;

        _isPrivateButton.interactable = false;
        _createButton.gameObject.SetActive(false);

        _readyButton.gameObject.SetActive(true);

        UpdatePlayersList();
    }

    private void JoinSetUp()
    {
        _localLobby = _gameManager.LocalLobby;

        _localLobby.onUserJoined += AddPlayer;
        _localLobby.onUserLeft += RemovePlayer;

        _readyButton.gameObject.SetActive(true);
        _createButton.gameObject.SetActive(false);
        _isPrivateButton.gameObject.SetActive(false);
    }

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
}
