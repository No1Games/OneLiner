using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _lobbyNameText;
    [SerializeField] private TextMeshProUGUI _playersText;

    [Space]
    [SerializeField] private Button _joinButton;

    private Lobby _lobby;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnClick_JoinButton);
    }

    public void UpdateLobby(Lobby lobby)
    {
        _lobby = lobby;

        _lobbyNameText.text = lobby.Name;
        _playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
    }

    private void OnClick_JoinButton()
    {
        try
        {
            LobbyManager.Instance.JoinLobby(_lobby);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
