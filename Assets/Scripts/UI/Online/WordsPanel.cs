using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsPanel : MonoBehaviour
{
    [Space]
    [SerializeField] private Button _closeBtn;

    [Space]
    [SerializeField] private WordButtonOnline _wordButtonPrefab;

    [Space]
    [SerializeField]
    private TurnHandler _turnHandler;

    private List<WordButtonOnline> _wordButtons;

    public event Action<int> UserClickedWordEvent;

    private void Awake()
    {
        _closeBtn.onClick.AddListener(OnClick_CloseButton);
    }

    public void SetButtons(List<string> words)
    {
        if (words == null || words.Count == 0)
        {
            Debug.LogWarning("Words list is null or empty!");
            return;
        }

        if (_wordButtons == null || _wordButtons.Count != words.Count)
        {
            _wordButtons = new List<WordButtonOnline>();
            InstantiateButtons(words.Count);
        }

        for (int i = 0; i < words.Count; i++)
        {
            _wordButtons[i].SetWord(words[i], i);
            _wordButtons[i].gameObject.SetActive(true);
            _wordButtons[i].WordButtonClick += OnUserClickedWord;
            _wordButtons[i].Interactable = true;
        }
    }

    private void OnUserClickedWord(int index)
    {
        if (OnlineController.Instance.LocalLobby.LeaderID.Value == OnlineController.Instance.LocalPlayer.PlayerId.Value)
        {
            Debug.Log("You are a leader! You can't guess words!");
            return;
        }

        if (!_turnHandler.CurrentPlayerId.Value.ToString().Equals(OnlineController.Instance.LocalPlayer.PlayerId.Value))
        {
            Debug.Log("You can't guess words for now! Wait for your move!");
            return;
        }

        _wordButtons[index].DisableButton();

        UserClickedWordEvent?.Invoke(index);

        Hide();
    }

    private void InstantiateButtons(int count)
    {
        for (int i = 0; i < count; i++)
        {
            WordButtonOnline wordButton = Instantiate(_wordButtonPrefab, transform);
            wordButton.gameObject.SetActive(false);
            _wordButtons.Add(wordButton);
        }
    }

    public void SetLeaderWord(int leaderWordIndex)
    {
        if (OnlineController.Instance.LocalPlayer.Role.Value != PlayerRole.Leader) return;

        if (leaderWordIndex > _wordButtons.Count || leaderWordIndex < 0)
        {
            Debug.LogWarning("Invalid Leader Word Index!");
            return;
        }

        _wordButtons[leaderWordIndex].SetLeaderWord();
    }

    private void OnClick_CloseButton()
    {
        Hide();
    }

    public void DisableButton(int index)
    {
        _wordButtons[index].DisableButton();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
