using System.Collections.Generic;
using UnityEngine;

public class WordManager : MonoBehaviour
{
    //Додати тут дікшенарі Вордлістов чи просто тримати тут перелік цих вордлістов в залежності від скланості чи теми.
    [SerializeField] private WordList wordList;

    [SerializeField] private int wordAmount;

    System.Random rand = new System.Random();

    string GetRandomWord(List<string> wordsToChoose)
    {
        int wordIndex = rand.Next(0, wordsToChoose.Count);
        return wordsToChoose[wordIndex];
    }

    public List<string> FormWordListForRound() //Переробити так щоб він приймав WordList і обирав там потрібний список в залежності від  умов
    {
        List<string> words = new List<string>();
        for (int i = 0; i < wordAmount; i++)
        {
            string tempWord = GetRandomWord(wordList.words);
            while (words.Contains(tempWord))
            {
                tempWord = GetRandomWord(wordList.words);
            }

            words.Add(tempWord);

        }
        return new List<string>(words);
    }

    public string GetLeaderWord(List<string> words)
    {
        return GetRandomWord(words);
    }


    #region OnlineCode
    public int GetLeaderWordIndex(List<string> words)
    {
        return Random.Range(0, words.Count);
    }

    public List<int> GetWordsIndexes(List<string> words)
    {
        List<int> indexes = new List<int>();

        foreach (string word in words)
        {
            int index = wordList.words.IndexOf(word);
            indexes.Add(index);
        }

        return indexes;
    }

    public List<string> GetWordsFromIndexes(List<int> indexes)
    {
        List<string> words = new List<string>();

        foreach (int index in indexes)
        {
            words.Add(wordList.words[index]);
        }

        return words;
    }
    #endregion
}
