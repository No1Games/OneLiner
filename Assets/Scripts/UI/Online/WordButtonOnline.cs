using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class WordButtonOnline : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _wordTMP;
    [SerializeField] private LocalizeStringEvent _localize;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _leaderIcon;
    [SerializeField] private GameObject _wrongIcon;

    private int _index;
    private Action<int> _onClickCallback;

    private void Awake()
    {
        _button.onClick.AddListener(() => _onClickCallback?.Invoke(_index));
    }

    public void Init(string word, int index, Action<int> onClick)
    {
        _localize.StringReference.TableEntryReference = word;
        _index = index;
        _onClickCallback = onClick;
        _leaderIcon.SetActive(false);
        _wrongIcon.SetActive(false);
        _button.interactable = true;
    }

    public void SetLeaderWord() => _leaderIcon.SetActive(true);

    public void DisableButton()
    {
        _wrongIcon.SetActive(true);
        _button.interactable = false;
    }
}