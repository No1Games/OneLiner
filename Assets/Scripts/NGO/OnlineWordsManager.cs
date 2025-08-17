using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnlineWordsManager : NetworkBehaviour
{
    [SerializeField]
    private WordsPanel _wordsPanel;

    [SerializeField]
    private WordManager _wordManager;

    public NetworkList<int> WordsIndexes = new NetworkList<int>();

    public override void OnNetworkSpawn()
    {
        Debug.Log($"OnNetworkSpawn called on OnlineWordsManager. Is Host: {OnlineController.Instance.LocalPlayer.IsHost.Value}");

        if (OnlineController.Instance.LocalPlayer.IsHost.Value)
        {
            var wordsList = _wordManager.FormWordListForRound();
            var indexes = _wordManager.GetWordsIndexes(wordsList);

            WordsIndexes.Clear();
            foreach (var index in indexes)
            {
                WordsIndexes.Add(index);
            }
        }

        SetButtonsRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetButtonsRpc()
    {
        Debug.Log("SetButtonsRpc is called");

        List<int> indexes = new List<int>();

        foreach (var index in WordsIndexes)
        {
            indexes.Add(index);
        }

        _wordsPanel.SetButtons(_wordManager.GetWordsFromIndexes(indexes));
    }
}
