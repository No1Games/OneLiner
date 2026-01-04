using System.Collections.Generic;
using UnityEngine;

public class WordsInitializer : MonoBehaviour
{
    // Reference to word manager
    [SerializeField] private WordManager _wordManager;

    public int WordsAmount => _wordManager.WordsAmount;

    public List<string> GetWordsForRound()
    {
        return _wordManager.FormWordListForRound();
    }

    public List<int> GetWordsIndexes(List<string> words)
    {
        return _wordManager.GetWordsIndexes(words);
    }

    public int GetLeaderWordIndex(List<string> words)
    {
        return Random.Range(0, words.Count);
    }

    public List<string> GetWordsFromIndexes(List<int> indexes)
    {
        return _wordManager.GetWordsFromIndexes(indexes);
    }
}
