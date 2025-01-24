using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countDownTMP;

    public void Show()
    {
        _countDownTMP.enabled = true;
    }

    public void Hide()
    {
        _countDownTMP.enabled = false;
    }

    public void OnTimeChanged(float time)
    {
        _countDownTMP.SetText($"{time:0}");
    }
}
