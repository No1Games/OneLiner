using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TurnHandler : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] ILogger _logger;

    [SerializeField] private TextMeshProUGUI _currentPlayerTMP;
    [SerializeField] private Button _drawButton;

    private LocalLobby _localLobby;

    private Queue<LocalPlayer> _playersQueue;
    private LocalPlayer _leader;

    private LocalPlayer _currentPlayer;
    public LocalPlayer CurrentPlayer => _currentPlayer;

    private void Start()
    {
        // Cache current lobby
        _localLobby = GameManager.Instance.LocalLobby;

        // Subscribe on turn changing
        _localLobby.CurrentPlayerID.onChanged += OnTurnIDChanged;
        GameManager.Instance.PassTurn += PassTurn;


        // Following actions makes only host
        if (GameManager.Instance.LocalUser.IsHost.Value)
        {
            // Form players queue and find leader
            CreateQueue();

            // Leader first to take turn
            GameManager.Instance.SetTurnID(_leader.ID.Value);
        }
    }

    private void PassTurn()
    {
        LocalPlayer nextPlayer = null;

        if (_currentPlayer.Role.Value == PlayerRole.Player)
        {
            _playersQueue.Enqueue(_currentPlayer);
            nextPlayer = _leader;
        }
        else if (_currentPlayer.Role.Value == PlayerRole.Leader)
        {
            nextPlayer = _playersQueue.Dequeue();
        }

        if (nextPlayer == null)
        {
            _logger.Log($"Failed to calculate next player");
        }

        GameManager.Instance.SetTurnID(nextPlayer.ID.Value);
    }

    private void OnTurnIDChanged(string newID)
    {
        // Get current player from lobby
        _currentPlayer = _localLobby.GetLocalPlayer(newID);

        if (_currentPlayer == null)
        {
            _logger.Log($"Failed to find local player with id {newID}");
            return;
        }

        // Update UI
        SetUI();
    }

    private void CreateQueue()
    {
        _playersQueue = new Queue<LocalPlayer>();

        foreach (var p in _localLobby.LocalPlayers)
        {
            if (p.Role.Value == PlayerRole.Leader)
            {
                _leader = p;
                continue;
            }
            _playersQueue.Enqueue(p);
        }
    }

    private void SetUI()
    {
        _currentPlayerTMP.text = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value ? "Ви" : _currentPlayer.DisplayName.Value;

        _drawButton.interactable = _currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value;
    }

    public void EndTurn()
    {
        GameManager.Instance.SetLocalUserTurn(false);
    }
}

