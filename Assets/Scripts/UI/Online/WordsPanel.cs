using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsPanel : MonoBehaviour
{
    [SerializeField] private Button _closeBtn;

    [SerializeField] private WordButton _wordButtonPrefab;

    private List<WordButton> _buttonsPool = new List<WordButton>();

    public event Action<int> UserClickedWord;

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
            _buttonsPool[i].SetWord(words[i], i);
            _buttonsPool[i].gameObject.SetActive(true);
            _buttonsPool[i].WordButtonClick += OnUserClickedWord;
        }
    }

    private void OnUserClickedWord(int index)
    {
        UserClickedWord?.Invoke(index);
        Hide();
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

        if (leaderWordIndex > _buttonsPool.Count || leaderWordIndex < 0)
        {
            Debug.Log("Invalid Leader Word Index");
            return;
        }

        _buttonsPool[leaderWordIndex].SetLeaderWord();
    }

    private void OnClick_CloseButton()
    {
        Hide();
    }

    public void DisableButton(int index)
    {
        _buttonsPool[index].Interactable = false;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
