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
        if (time <= 0)
            _countDownTMP.SetText("Waiting for all players...");
        else
            _countDownTMP.SetText($"Starting in: {time:0}");
    }
}
