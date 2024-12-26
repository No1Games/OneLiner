using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countDownTMP;

    public void Show()
    {
        Debug.Log("Countdown ui show");
        _countDownTMP.enabled = true;
    }

    public void Hide()
    {
        Debug.Log("Countdown ui hide");
        _countDownTMP.enabled = false;
    }

    public void OnTimeChanged(float time)
    {
        _countDownTMP.SetText($"{time:0}");
    }
}
