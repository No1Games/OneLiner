using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnPasser : MonoBehaviour
{
    [SerializeField] private TurnUI _view;

    private string _leaderId;
    private string _currentPlayerId;
    public string CurrentPlayerId => _currentPlayerId;
    private Queue<string> _players = new Queue<string>();

    public Action<string> TurnIdSet;

    private OnlineController _controller;

    private void Awake()
    {
        _controller = OnlineController.Instance;
    }

    public void Initialize()
    {
        _leaderId = _controller.LocalLobby.LeaderID.Value;

        CreateQueue();

        TurnIdSet.Invoke(_leaderId);
    }

    private void CreateQueue()
    {
        _players = new Queue<string>();

        foreach (var p in _controller.LocalLobby.LocalPlayers)
        {
            if (p.PlayerId.Value == _leaderId.ToString()) continue;

            _players.Enqueue(p.PlayerId.Value);
        }
    }

    public void SetTurnTo(string id)
    {
        _currentPlayerId = id;
        _view.UpdateUI(_controller.LocalPlayer.PlayerId.Value.Equals(id));
    }

    public void PassTurn()
    {
        string nextId;

        if (_leaderId == _currentPlayerId)
        {
            nextId = _players.Dequeue();
        }
        else
        {
            _players?.Enqueue(_currentPlayerId);
            nextId = _leaderId;
        }

        TurnIdSet?.Invoke(nextId);
    }
}
