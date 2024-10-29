using TMPro;
using UnityEngine;

public class WordButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _wordTMP;

    public void SetWord(string word)
    {
        _wordTMP.text = word;
    }
}