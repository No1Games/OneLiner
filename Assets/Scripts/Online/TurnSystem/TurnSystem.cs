using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    [SerializeField] private TurnPasser _passer;

    [SerializeField] private OnlineGameManager _manager;

    public string CurrentPlayerId => _passer.CurrentPlayerId;

    private void Start()
    {
        _passer.TurnIdSet += OnTurnIdSet;

        _passer.Initialize();
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
}
