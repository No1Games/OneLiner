using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public class WordManager : MonoBehaviour
{
    //������ ��� ������� ��������� �� ������ ������� ��� ������ ��� ��������� � ��������� �� ���� �������� �� ����.
    [SerializeField] private WordList wordList;

    [SerializeField] private int wordAmount;

    System.Random rand = new System.Random();

    string GetRandomWord(List<string> wordsToChoose)
    {

        int wordIndex = rand.Next(0, wordsToChoose.Count);
        return wordsToChoose[wordIndex];
    }

    public List<string> FormWordListForRound() //���������� ��� ��� �� ������� WordList � ������ ��� �������� ������ � ��������� ��  ���� ���� �� ����
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
