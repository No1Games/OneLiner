using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WordsSystem : MonoBehaviour
{
    [SerializeField] private WordsUI _wordsUI;
    [SerializeField] private WordsNetworkStorage _networkStorage;
    [SerializeField] private WordsInitializer _wordsInitializer;

    private WordsLocalStorage _localStorage;

    public int LeaderWordIndex => _networkStorage.LeaderWordIndex;
    public string LeaderWord => _localStorage.LeaderWord;

    private void Awake()
    {
        _localStorage = new WordsLocalStorage();
    }

    public void InitilizeSystem(Action<int> onWordClicked)
    {
        List<string> words = _wordsInitializer.GetWordsForRound();
        List<int> indexes = _wordsInitializer.GetWordsIndexes(words);
        int leaderWordIndex = _wordsInitializer.GetLeaderWordIndex(words);
        
        _networkStorage.UpdateStorage(indexes, leaderWordIndex);

        _wordsUI.Init(onWordClicked);
    }

    public void OnWordsReady()
    {
        List<string> words = _wordsInitializer.GetWordsFromIndexes(_networkStorage.WordIndexes);
        int leaderWordIndex = _networkStorage.LeaderWordIndex;

        _localStorage.Update(words, leaderWordIndex);

        _wordsUI.SetButtons(words);
        _wordsUI.SetLeaderWord(leaderWordIndex);
    }

    public void DisableButton(int index)
    {
        _wordsUI.DisableButton(index);
    }
}
