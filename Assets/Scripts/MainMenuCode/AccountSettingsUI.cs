using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountSettingsUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;

    [SerializeField] private Button _confirmNameBtn;

    private string _playerName;

    private void Start()
    {
        _confirmNameBtn.interactable = false;
        _confirmNameBtn.onClick.AddListener(OnConfirmButtonClick);
        _nameInput.onValueChanged.AddListener(OnNameInputValueChanged);
    }

    private void OnNameInputValueChanged(string value)
    {
        _playerName = value;
        _confirmNameBtn.interactable = !_playerName.IsEmpty();
    }

    private async void OnConfirmButtonClick()
    {
        // OnlineController.Instance.SetLocalPlayerName(_playerName);
        await OnlineController.Instance.LobbyManager.LocalPlayerEditor
            .SetDisplayName(_playerName)
            .CommitChangesAsync();
    }
}
