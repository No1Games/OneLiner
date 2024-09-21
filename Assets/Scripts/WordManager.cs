using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public class WordManager : MonoBehaviour
{
    //Додати тут дікшенарі Вордлістов чи просто тримати тут перелік цих вордлістов в залежності від мови скланості чи теми.
    [SerializeField] private WordList wordList;

    [SerializeField] private int wordAmount;

    System.Random rand = new System.Random();

    string GetRandomWord(List<string> wordsToChoose)
    {

        int wordIndex = rand.Next(0, wordsToChoose.Count);
        return wordsToChoose[wordIndex];
    }

    public List<string> FormWordListForRound() //Переробити так щоб він приймав WordList і обирав там потрібний список в залежності від  умов теми та мови
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

}
