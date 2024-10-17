using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AuthenticateUI : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _logInButton;

    private string _playerNameStr;

    private void Start()
    {
        _logInButton.interactable = false;
        _playerNameInput.onValueChanged.AddListener(OnValueChanged_PlayerNameInput);
        _logInButton.onClick.RemoveAllListeners();
        _logInButton.onClick.AddListener(OnClick_LogInButton);
    }

    private void OnValueChanged_PlayerNameInput(string value)
    {
        _playerNameStr = value;
        _logInButton.interactable = !value.Equals(string.Empty);
    }

    private void OnClick_LogInButton()
    {
        GameManager.Instance.SetLocalUserName(_playerNameStr);

        gameObject.SetActive(false);
        LobbyListUI.Instance.Show();
    }
}