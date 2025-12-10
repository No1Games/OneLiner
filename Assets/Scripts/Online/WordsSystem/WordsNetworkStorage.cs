using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WordsNetworkStorage : NetworkBehaviour
{
    // Network variables to sync words list and leader word between players
    private NetworkList<int> _wordsIndexes = new NetworkList<int>();
    private NetworkVariable<int> _leaderWordIndex = new NetworkVariable<int>();

    public List<int> WordIndexes 
    {
        get
        {
            List<int> result = new List<int>();
            foreach (var index in _wordsIndexes)
            {
                result.Add(index);
            }
            return result;
        }
    }
    public int LeaderWordIndex => _leaderWordIndex.Value;

    public void UpdateStorage(List<int> wordsIndexes, int leaderWordIndex)
    {
        _wordsIndexes.Clear();
        foreach (var index in wordsIndexes)
        {
            _wordsIndexes.Add(index);
        }
        _leaderWordIndex.Value = leaderWordIndex;
    }
}
