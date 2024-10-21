using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinWithCodeUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _backButton;

    [SerializeField] private TextMeshProUGUI _connectionErrorMessage;

    private string _code;

    void Awake()
    {
        //LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
    }

    void Start()
    {
        _backButton.onClick.AddListener(OnClick_BackButton);
        _connectButton.onClick.AddListener(OnClick_ConnectButton);
        _connectButton.interactable = false;
        _codeInput.onValueChanged.AddListener(OnValueChanged_CodeInput);
    }

    private void OnValueChanged_CodeInput(string value)
    {
        _connectionErrorMessage.text = "";

        _code = value;

        _connectButton.interactable = _code != null;
    }

    private async void OnClick_ConnectButton()
    {
        try
        {
            //await LobbyManager.Instance.JoinLobbyByCode(_code);
        }
        catch (Exception e)
        {
            _codeInput.text = "";
            _code = null;
            _connectButton.interactable = false;
            _connectionErrorMessage.text = e.Message;
        }
    }

    private void OnClick_BackButton()
    {
        Hide();
    }

    #region Show / Hide

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
