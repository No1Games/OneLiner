using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _wordTMP;
    [SerializeField] private Image _image;

    public void SetWord(string word)
    {
        _wordTMP.text = word;
    }

    public void SetLeaderWord()
    {
        _image.color = Color.yellow;
    }
}