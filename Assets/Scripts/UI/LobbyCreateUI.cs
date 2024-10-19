using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MenuBase
{
    public static LobbyCreateUI Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private TMP_InputField _lobbyNameInput;
    [SerializeField] private TMP_InputField _maxPlayersInput;
    [SerializeField] private Toggle _isPrivateToggle;

    [Header("Buttons")]
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _backButton;

    public override MenuName Menu => MenuName.LobbyCreate;

    private string _lobbyName;
    private bool _isPrivate;
    private int _maxPlayers;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        _createButton.interactable = false;
    }

    #region Fields Listeners

    private void OnValueChanged_LobbyMenuInput(string value)
    {
        _lobbyName = value;
        _createButton.interactable = !_lobbyName.Equals(string.Empty) && _maxPlayers > 1;
    }

    private void OnValueChanged_MaxPlayersInput(string value)
    {
        _maxPlayers = Convert.ToInt32(value);
        _createButton.interactable = !_lobbyName.Equals(string.Empty) && _maxPlayers > 1;
    }

    private void OnValueChanged_IsPrivateToggle(bool value)
    {
        _isPrivate = value;
    }

    #endregion

    private void OnClick_CreateButton()
    {
        GameManager.Instance.CreateLobby(_lobbyName, _isPrivate, _maxPlayers);
    }

    private void OnClick_BackButton()
    {
        Hide();
        LobbyListUI.Instance.Show();
    }

    #region Menu Methods

    public override void Init()
    {
        Instance = this;

        _lobbyNameInput.onValueChanged.AddListener(OnValueChanged_LobbyMenuInput);
        _maxPlayersInput.onValueChanged.AddListener(OnValueChanged_MaxPlayersInput);
        _isPrivateToggle.onValueChanged.AddListener(OnValueChanged_IsPrivateToggle);

        _createButton.onClick.AddListener(OnClick_CreateButton);
        _backButton.onClick.AddListener(OnClick_BackButton);

        _isPrivate = _isPrivateToggle.isOn;
    }

    private void UpdateFields()
    {
        _lobbyNameInput.text = _lobbyName;
        _isPrivateToggle.isOn = _isPrivate;
        _maxPlayersInput.text = _maxPlayers.ToString();
    }

    #endregion
}
