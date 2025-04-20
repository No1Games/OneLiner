using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsPanel : MonoBehaviour
{
    [Space]
    [SerializeField] private Button m_CloseBtn;

    [Space]
    [SerializeField] private WordButtonOnline m_WordButtonPrefab;
    [SerializeField] private Transform m_ButtonsGridT;

    private List<WordButtonOnline> m_WordButtons;

    public event Action<int> UserClickedWordEvent;

    private void Awake()
    {
        m_CloseBtn.onClick.AddListener(OnClick_CloseButton);
    }

    public void SetButtons(List<string> words)
    {
        if (words == null || words.Count == 0)
        {
            Debug.LogWarning("Words list is null or empty!");
            return;
        }

        if (m_WordButtons == null || m_WordButtons.Count != words.Count)
        {
            m_WordButtons = new List<WordButtonOnline>();
            InstantiateButtons(words.Count);
        }

        for (int i = 0; i < words.Count; i++)
        {
            m_WordButtons[i].SetWord(words[i], i);
            m_WordButtons[i].gameObject.SetActive(true);
            m_WordButtons[i].WordButtonClick += OnUserClickedWord;
            m_WordButtons[i].Interactable = true;
        }
    }

    private void OnUserClickedWord(int index)
    {
        UserClickedWordEvent?.Invoke(index);

        Hide();
    }

    private void InstantiateButtons(int count)
    {
        for (int i = 0; i < count; i++)
        {
            WordButtonOnline wordButton = Instantiate(m_WordButtonPrefab, m_ButtonsGridT);
            wordButton.gameObject.SetActive(false);
            m_WordButtons.Add(wordButton);
        }
    }

    public void SetLeaderWord(int leaderWordIndex)
    {
        if (OnlineController.Instance.LocalPlayer.Role.Value != PlayerRole.Leader) return;

        if (leaderWordIndex > m_WordButtons.Count || leaderWordIndex < 0)
        {
            Debug.LogWarning("Invalid Leader Word Index!");
            return;
        }

        m_WordButtons[leaderWordIndex].SetLeaderWord();
    }

    private void OnClick_CloseButton()
    {
        Hide();
    }

    public void DisableButton(int index)
    {
        m_WordButtons[index].DisableButton();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
