using System.Collections.Generic;

public class WordsLocalStorage
{
    private List<string> _words = new List<string>();
    private int _leaderWordIndex;

    public List<string> Words => _words;
    public string LeaderWord => _words[_leaderWordIndex];

    public void Update(List<string> words, int leaderWordIndex)
    {
        _words = words;
        _leaderWordIndex = leaderWordIndex;
    }
}
