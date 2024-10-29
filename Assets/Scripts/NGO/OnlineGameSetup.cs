using System.Collections.Generic;
using UnityEngine;

public class OnlineGameSetup : MonoBehaviour
{
    [SerializeField] private StartTurnPanelUI _startTurnPanel;
    [SerializeField] private TurnHandler _turnHandler;

    [SerializeField] private WordsPanel _wordsPanel;
    [SerializeField] private WordManager _wordManager;
    private List<string> _words = new List<string>();

    private void Start()
    {
        _turnHandler.OnTurnChanged += _startTurnPanel.OnTurnChanged;

        _words = _wordManager.FormWordListForRound();
        _wordsPanel.InitPanel(_words);
    }
}
