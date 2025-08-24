using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineWordsManager : NetworkBehaviour
{
    // Reference to the UI
    [SerializeField]
    private WordsPanel _wordsPanel;

    // Reference to word manager
    [SerializeField]
    private WordManager _wordManager;

    // Network variables to sync words list and leader word between players
    public NetworkList<int> WordsIndexes = new NetworkList<int>();
    public NetworkVariable<int> LeaderWordIndex = new NetworkVariable<int>();

    private List<string> _words = new List<string>();
    public string LeaderWord => _words[LeaderWordIndex.Value];

    public override void OnNetworkSpawn()
    {
        // If host spawn manager generate words list and leader word
        if (OnlineController.Instance.LocalPlayer.IsHost.Value)
        {
            _words = _wordManager.FormWordListForRound();
            var indexes = _wordManager.GetWordsIndexes(_words);

            WordsIndexes.Clear();
            foreach (var index in indexes)
            {
                WordsIndexes.Add(index);
            }

            LeaderWordIndex.Value = _wordManager.GetLeaderWordIndex(_words);
        }

        // Call rpc events to both host and clients
        SetButtonsRpc(); // Sets words to buttons
        SetLeaderWordRpc(); // Sets leader word
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetButtonsRpc()
    {
        List<int> indexes = new List<int>();

        foreach (var index in WordsIndexes)
        {
            indexes.Add(index);
        }

        _wordsPanel.SetButtons(_wordManager.GetWordsFromIndexes(indexes));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetLeaderWordRpc()
    {
        _wordsPanel.SetLeaderWord(LeaderWordIndex.Value);
    }
}
