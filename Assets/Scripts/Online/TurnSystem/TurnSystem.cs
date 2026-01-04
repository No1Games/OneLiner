using System;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    [SerializeField] private TurnPasser _passer;

    [SerializeField] private OnlineSystemsMediator _manager;

    public string CurrentPlayerId => _passer.CurrentPlayerId;

    private void Start()
    {
        _passer.TurnIdSet += OnTurnIdSet;
    }

    private void OnDestroy()
    {
        _passer.TurnIdSet -= OnTurnIdSet;
    }

    private void OnTurnIdSet(string playerId)
    {
        _manager.SetTurnTo(playerId);
    }

    public void SetTurnTo(string playerId)
    {
        _passer.SetTurnTo(playerId);
    }

    public void EndTurn()
    {
        _passer.PassTurn();
    }

    internal void InitializeSystem()
    {
        _passer.Initialize();
    }
}
