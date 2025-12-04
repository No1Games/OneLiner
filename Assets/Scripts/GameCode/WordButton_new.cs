using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class WordButton_new : MonoBehaviour
{
    [SerializeField] private Button wordButton;
    [SerializeField] private GameObject selectedWordImg;
    [SerializeField] private GameObject wrongWordImg;
    
    [SerializeField] private LocalizeStringEvent localizeStringEvent; 
    private string key;

    public event Action<WordButton_new> wordClicked;

    private void Start()
    {
        wordButton.onClick.AddListener(OnBtnClick);
    }
    

    public void SetKey(string newKey)
    {
        key = newKey; 
        if (localizeStringEvent != null)
        {
            localizeStringEvent.StringReference.TableEntryReference = key; 
        }
        else
        {
            Debug.LogWarning("LocalizeStringEvent не прив'язаний до об'єкта кнопки.");
        }
    }

    public string GetKey()
    {
        return key;
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
