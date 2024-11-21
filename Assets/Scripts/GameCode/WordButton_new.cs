using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButton_new : MonoBehaviour
{
    [SerializeField] private Button wordButton;
    [SerializeField] private GameObject selectedWordImg;
    [SerializeField] private GameObject wrongWordImg;
    [SerializeField] private TMP_Text wordText;
    
    public event Action<WordButton_new> wordClicked;

    private void Start()
    {
        wordButton.onClick.AddListener(OnBtnClick);
    }
    public void SetWord(string word)
    {
        wordText.text = word;
        
    }

    public void ShowLeaderWord()
    {
        selectedWordImg.SetActive(true);
    }
    public void HideLeaderWord()
    {
        selectedWordImg.SetActive(false);
    }
    public void ShowWordAsWrong()
    {
        wrongWordImg.SetActive(true);
        wordButton.onClick.RemoveAllListeners();

    }
    private void OnBtnClick()
    {
        wordClicked?.Invoke(this);
    }


}
