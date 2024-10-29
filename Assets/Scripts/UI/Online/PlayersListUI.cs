using System.Collections.Generic;
using UnityEngine;

public class PlayersListUI : MonoBehaviour
{
    [SerializeField] private PlayerPanelUI _playerPrefab;

    private List<PlayerPanelUI> _playersPool = new List<PlayerPanelUI>();

    private LocalLobby _lobby;

    private void Start()
    {
        _lobby = GameManager.Instance.LocalLobby;

        SetPlayersUI();
    }

    private void SetPlayersUI()
    {
        if (_playersPool.Count < _lobby.LocalPlayers.Count)
        {
            InstantiatePlayers(_lobby.LocalPlayers.Count - _playersPool.Count);
        }

        for (int i = 0; i < _lobby.LocalPlayers.Count; i++)
        {
            _playersPool[i].gameObject.SetActive(true);
            _playersPool[i].SetLocalPlayer(_lobby.GetLocalPlayer(i));
        }
    }

    private void InstantiatePlayers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiatePlayer();
        }
    }

    private void InstantiatePlayer()
    {
        PlayerPanelUI playerPanel = Instantiate(_playerPrefab, transform);
        playerPanel.gameObject.SetActive(false);
        _playersPool.Add(playerPanel);
    }
}
