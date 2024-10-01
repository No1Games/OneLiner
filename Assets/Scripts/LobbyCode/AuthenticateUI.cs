using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Button _logInButton;

    private string _playerNameStr;

    private void Start()
    {
        _logInButton.interactable = false;
        _playerNameInput.onValueChanged.AddListener(OnValueChanged_PlayerNameInput);
        _logInButton.onClick.AddListener(OnClick_LogInButton);
    }

    private void OnValueChanged_PlayerNameInput(string value)
    {
        _playerNameStr = value;
        _logInButton.interactable = !value.Equals(string.Empty);
    }

    private void OnClick_LogInButton()
    {
        LobbyManager.Instance.Authenticate(_playerNameStr);
    }
}