using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _playersText;

    [Space]
    [SerializeField] private Button _joinButton;

    private LocalLobby _localLobby;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnClick_JoinButton);
    }

    public void SetLocalLobby(LocalLobby lobby)
    {
        _localLobby = lobby;

        UpdateUI();
    }

    private void UpdateUI()
    {
        _lobbyNameText.text = _localLobby.LobbyName.Value;
        _playersText.text = $"{_localLobby.PlayerCount}/{_localLobby.MaxPlayerCount.Value}";
    }

    private void OnClick_JoinButton()
    {
        try
        {
            GameManager.Instance.JoinLobby(_localLobby.LobbyID.Value, _localLobby.LobbyCode.Value);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
