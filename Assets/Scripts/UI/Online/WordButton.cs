using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _wordTMP;
    [SerializeField] private Image _image;

    private Button _buttonComponent;

    private string _word;

    public event Action<string> WordButtonClick;

    public void SetWord(string word)
    {
        _wordTMP.text = word;
        _word = word;
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

        WordButtonClick?.Invoke(_word);
    }
}