using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineGameSetup : NetworkBehaviour
{
    [SerializeField] private StartTurnPanelUI _startTurnPanel;
    [SerializeField] private TurnHandler _turnHandler;

    [SerializeField] private WordsPanel _wordsPanel;
    [SerializeField] private WordManager _wordManager;
    private List<string> _words = new List<string>();
    private List<int> _wordsIndexes = new List<int>();
    private int _leaderWord;

    private LocalLobby _localLobby;
    private LocalPlayer _localPlayer;

    private LocalPlayer _leader;
    private LocalPlayer _host;

    private void Start()
    {
        _localLobby = GameManager.Instance.LocalLobby;
        _localPlayer = GameManager.Instance.LocalUser;

        _leader = _localLobby.LocalPlayers.Find(p => p.Role.Value == PlayerRole.Leader);
        _host = _localLobby.LocalPlayers.Find(_p => _p.IsHost.Value == true);

        _turnHandler.OnTurnChanged += _startTurnPanel.OnTurnChanged;

        _localLobby.WordsList.onChanged += OnWordsChanged;
        _localLobby.LeaderWord.onChanged += OnLeaderWordChanged;

        if (_localPlayer.Role.Value == PlayerRole.Leader)
        {
            _words = _wordManager.FormWordListForRound();
            _wordsIndexes = _wordManager.GetWordsIndexes(_words);
            _leaderWord = _wordManager.GetLeaderWordIndex(_words);
            GameManager.Instance.SetWordsList(_wordsIndexes, _leaderWord);
        }
    }

    private void OnWordsChanged(List<int> indexes)
    {
        _words = _wordManager.GetWordsFromIndexes(indexes);

        _wordsPanel.SetButtons(_words);
    }

    private void OnLeaderWordChanged(int index)
    {
        _wordsPanel.SetLeaderWord(index);
    }
}
