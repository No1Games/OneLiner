using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    [SerializeField]
    TMP_Text _countDownText;

    public void OnTimeChanged(float time)
    {
        if (time <= 0)
            _countDownText.SetText("Waiting for all players...");
        else
            _countDownText.SetText($"Starting in: {time:0}"); // Note that the ":0" formatting rounds, not truncates.
    }
}
