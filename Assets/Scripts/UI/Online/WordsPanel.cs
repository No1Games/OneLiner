using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordsPanel : MonoBehaviour
{
    [SerializeField] private Button _closeBtn;
    [SerializeField] private WordButtonOnline _buttonPrefab;

    private readonly List<WordButtonOnline> _buttons = new();
    private Action<int> _onClickCallback;

    private void Awake() => _closeBtn.onClick.AddListener(Hide);

    private void EnsureButtonCount(int count)
    {
        while (_buttons.Count < count)
        {
            var btn = Instantiate(_buttonPrefab, transform);
            btn.gameObject.SetActive(false);
            _buttons.Add(btn);
        }
    }

    public void Init(Action<int> onWordClicked)
    {
        _onClickCallback = onWordClicked;
    }

    public void SetButtons(List<string> words)
    {
        if (words == null || words.Count == 0)
        {
            Debug.LogWarning("WORDS WARNING: Empty words list!");
            return;
        }

        EnsureButtonCount(words.Count);

        for (int i = 0; i < words.Count; i++)
        {
            _buttons[i].gameObject.SetActive(true);
            _buttons[i].Init(words[i], i, _onClickCallback);
        }
    }

    public void SetLeaderWord(int index)
    {
        if (index < 0 || index >= _buttons.Count)
        {
            Debug.LogWarning("WORDS WARNING: Invalid leader word index!");
            return;
        }

        if (OnlineController.Instance.LocalPlayer.Role.Value == PlayerRole.Leader)
            _buttons[index].SetLeaderWord();

    }

    public void DisableButton(int index)
    {
        if (index < 0 || index >= _buttons.Count) return;
        _buttons[index].DisableButton();
    }

    public void Hide() => gameObject.SetActive(false);
}
