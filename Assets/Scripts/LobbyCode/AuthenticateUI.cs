using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AuthenticateUI : MenuBase
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _logInButton;

    private string _playerNameStr;

    public override MenuName Menu => MenuName.Auth;

    private void Start()
    {
        Init();
    }

    private void OnValueChanged_PlayerNameInput(string value)
    {
        _playerNameStr = value;
        _logInButton.interactable = !value.Equals(string.Empty);
    }

    private void OnClick_LogInButton()
    {
        GameManager.Instance.SetLocalUserName(_playerNameStr);
        MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);

        //gameObject.SetActive(false);
        //LobbyListUI.Instance.Show();
    }

    public override void Init()
    {
        _logInButton.interactable = false;
        _playerNameInput.onValueChanged.AddListener(OnValueChanged_PlayerNameInput);
        _logInButton.onClick.RemoveAllListeners();
        _logInButton.onClick.AddListener(OnClick_LogInButton);
    }
}