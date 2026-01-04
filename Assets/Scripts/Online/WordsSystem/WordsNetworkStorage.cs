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

    private int _wordsAmount;
    public int WordsAmount { set { _wordsAmount = value; } }

    private bool _wordsReady = false;
    private bool _leaderWordReady = false;
    public event Action WordsArrived;

    public override void OnNetworkSpawn()
    {
        _wordsIndexes.OnListChanged += OnWordsChanged;
        _leaderWordIndex.OnValueChanged += OnLeaderWordChanged;
    }

    public override void OnNetworkDespawn()
    {
        _wordsIndexes.OnListChanged -= OnWordsChanged;
        _leaderWordIndex.OnValueChanged -= OnLeaderWordChanged;
    }

    public void UpdateStorage(List<int> wordsIndexes, int leaderWordIndex)
    {
        _wordsIndexes.Clear();
        foreach (var index in wordsIndexes)
        {
            _wordsIndexes.Add(index);
        }
        _leaderWordIndex.Value = leaderWordIndex;
    }

    private void OnLeaderWordChanged(int previousValue, int newValue)
    {
        _leaderWordReady = true;

        OnNetworkUpdate();
    }

    private void OnWordsChanged(NetworkListEvent<int> changeEvent)
    {
        if (_wordsIndexes.Count == _wordsAmount)
        {
            _wordsReady = true;
        }
        OnNetworkUpdate();
    }

    private void OnNetworkUpdate()
    {
        if (_wordsReady && _leaderWordReady)
        {
            WordsArrived.Invoke();
        }
    }
}
