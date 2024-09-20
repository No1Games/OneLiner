using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordManager : MonoBehaviour
{
    [SerializeField] private GameObject wordScreen;
    [SerializeField] private GameObject wordButtonPrefab;
    [SerializeField] private WordList wordList;
    [SerializeField] private int wordAmount;
    private List<GameObject> wordsButtons = new();
    private List<string> wordsForRound = new();

    System.Random rand = new System.Random();

    private string leaderWord;
    [SerializeField] TMP_Text wordForLeader;

    public event Action<bool> gameEnded;
    

    private void Awake()
    {
        FormWordListForRound();
        GenerateWordsForRound();
        leaderWord = GetRandomWord(wordsForRound);
        wordForLeader.text = leaderWord;


    }
    void GenerateWordsForRound()
    {
        
        for (int i = 0; i < wordAmount; i++)
        {

            GameObject newButton = Instantiate(wordButtonPrefab, wordScreen.transform);
            newButton.GetComponentInChildren<TMP_Text>().text = wordsForRound[i];
            wordsButtons.Add(newButton);

            Button button = newButton.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => CheckTheWord(newButton.GetComponentInChildren<TMP_Text>().text));
        }

    }   
    string GetRandomWord(List<string> wordsToChoose)
    {
        
        int wordIndex = rand.Next(0, wordsToChoose.Count);
        return wordsToChoose[wordIndex];
    }

    void FormWordListForRound()
    {
        for (int i = 0; i < wordAmount; i++)
        {
             string tempWord = GetRandomWord(wordList.words);
             while (wordsForRound.Contains(tempWord))
                {
                    tempWord = GetRandomWord(wordList.words);
                }

             wordsForRound.Add(tempWord);
            
        }
    }

    void CheckTheWord(string buttonText)
    {
        if( buttonText == leaderWord)
        {
            gameEnded.Invoke(true);
        }
        else
        {
            gameEnded.Invoke(false);
        }
    }


}
