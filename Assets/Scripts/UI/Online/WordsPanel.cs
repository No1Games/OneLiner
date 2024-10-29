using System.Collections.Generic;
using UnityEngine;

public class WordsPanel : MonoBehaviour
{
    [SerializeField] private WordButton _wordButtonPrefab;

    public void InitPanel(List<string> words)
    {
        foreach (string word in words)
        {
            WordButton wordButton = Instantiate(_wordButtonPrefab, transform);
            wordButton.SetWord(word);
        }
    }
}
