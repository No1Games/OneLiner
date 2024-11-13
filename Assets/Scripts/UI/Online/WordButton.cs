using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _wordTMP;
    [SerializeField] private Image _image;
    [SerializeField] private Button _buttonComponent;

    private int _index;

    public event Action<int> WordButtonClick;

    public bool Interactable
    {
        get { return _buttonComponent.interactable; }
        set { _buttonComponent.interactable = value; }
    }

    private void Start()
    {
        _buttonComponent.onClick.AddListener(OnClick);
    }

    public void SetWord(string word, int index)
    {
        _wordTMP.text = word;
        _index = index;
    }

    public void SetLeaderWord()
    {
        _image.color = Color.yellow;
    }

    private void OnClick()
    {
        if (GameManager.Instance.LocalUser.Role.Value == PlayerRole.Leader)
            return;

        if (!GameManager.Instance.LocalUser.IsTurn.Value)
            return;

        _buttonComponent.interactable = false;

        WordButtonClick?.Invoke(_index);
    }
}