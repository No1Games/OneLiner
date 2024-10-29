using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentPlayerTMP;
    [SerializeField] private Button _drawButton;

    private LocalLobby _localLobby;
    private Queue<LocalPlayer> _players = new Queue<LocalPlayer>();
    private LocalPlayer _leader;

    private LocalPlayer _currentPlayer;
    public LocalPlayer CurrentPlayer => _currentPlayer;

    public event Action<LocalPlayer> OnTurnChanged;

    private void Start()
    {
        _localLobby = GameManager.Instance.LocalLobby;

        _players = new Queue<LocalPlayer>(_localLobby.LocalPlayers.Where(player => player.Role.Value != PlayerRole.Leader));

        _leader = _localLobby.LocalPlayers.Find(player => player.Role.Value == PlayerRole.Leader);

        _currentPlayer = _leader;

        _currentPlayer.IsPlayerTurn.Value = true;

        _currentPlayerTMP.text = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value ? "Ви" : _currentPlayer.DisplayName.Value;

        _drawButton.interactable = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value;

        OnTurnChanged?.Invoke(_currentPlayer);
    }

    private void ChangeTurn()
    {
        _currentPlayer.IsPlayerTurn.Value = false;

        if (_currentPlayer.Equals(_leader))
        {
            _currentPlayer = _players.Dequeue();
        }
        else
        {
            _players.Enqueue(_currentPlayer);
            _currentPlayer = _leader;
        }

        _currentPlayerTMP.text = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value ? "Ви" : _currentPlayer.DisplayName.Value;

        _drawButton.interactable = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value;

        _currentPlayer.IsPlayerTurn.Value = true;

        OnTurnChanged?.Invoke(_currentPlayer);
    }
}
