using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsPanel : MonoBehaviour
{
    [SerializeField] private Button _closeBtn;

    [SerializeField] private WordButton _wordButtonPrefab;

    private List<WordButton> _buttonsPool = new List<WordButton>();

    private void Start()
    {
        _closeBtn.onClick.AddListener(OnClick_CloseButton);
    }

    public void SetButtons(List<string> words)
    {
        if (words == null)
        {
            Debug.Log("Failed to find words list!");
            return;
        }

        if (_buttonsPool.Count < words.Count)
        {
            InstantiateButtons(words.Count - _buttonsPool.Count);
        }

        for (int i = 0; i < words.Count; i++)
        {
            _buttonsPool[i].SetWord(words[i]);
            _buttonsPool[i].gameObject.SetActive(true);
        }
    }

    private void InstantiateButtons(int count)
    {
        for (int i = 0; i < count; i++)
        {
            WordButton wordButton = Instantiate(_wordButtonPrefab, transform);
            wordButton.gameObject.SetActive(false);
            _buttonsPool.Add(wordButton);
        }
    }

    public void SetLeaderWord(int leaderWordIndex)
    {
        if (GameManager.Instance.LocalUser.Role.Value != PlayerRole.Leader) return;

        try
        {
            _buttonsPool[leaderWordIndex].SetLeaderWord();
        }
        catch (Exception e)
        {
            Debug.Log("Invalid Leader Word Index");
        }
    }

    private void OnClick_CloseButton()
    {
        gameObject.SetActive(false);
    }
}
