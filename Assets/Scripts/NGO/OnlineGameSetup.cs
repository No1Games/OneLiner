using UnityEngine;

public class OnlineGameSetup : MonoBehaviour
{
    [SerializeField] private StartTurnPanelUI _startTurnPanel;
    [SerializeField] private TurnHandler _turnHandler;

    private void Start()
    {
        _turnHandler.OnTurnChanged += _startTurnPanel.OnTurnChanged;
    }
}
