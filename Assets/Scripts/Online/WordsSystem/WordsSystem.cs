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

    public void InitSystem(Func<int, bool> onWordClicked)
    {
        _localStorage = new WordsLocalStorage();

        _wordsUI.Init(onWordClicked);

        _networkStorage.WordsArrived += OnWordsReady;
    }

    public void InitilizeNetwork()
    {
        List<string> words = _wordsInitializer.GetWordsForRound();
        List<int> indexes = _wordsInitializer.GetWordsIndexes(words);
        int leaderWordIndex = _wordsInitializer.GetLeaderWordIndex(words);

        _networkStorage.UpdateStorage(indexes, leaderWordIndex);
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
