using System;
using UnityEngine;

/// <summary>
/// Runs the countdown to the in-game state. While the start of the countdown is synced via Relay, the countdown itself is handled locally,
/// since precise timing isn't necessary.
/// </summary>
public class Countdown : MonoBehaviour
{
    CallbackValue<float> TimeLeft = new CallbackValue<float>();

    [SerializeField] private CountdownUI _ui;
    private const int _countdownTime = 5;

    public event Action CountdownFinishedEvent;

    public void Start()
    {
        TimeLeft.onChanged += _ui.OnTimeChanged;
        TimeLeft.Value = -1;

        _ui.Hide();
    }

    private void OnDestroy()
    {
        TimeLeft.onChanged -= _ui.OnTimeChanged;
    }

    public void StartCountDown()
    {
        Debug.Log("Start countdown");

        TimeLeft.Value = _countdownTime;
        _ui.Show();
    }

    public void CancelCountDown()
    {
        Debug.Log("Cancel countdown");
        TimeLeft.Value = -1;

        _ui.Hide();
    }

    public void Update()
    {
        if (TimeLeft.Value < 0)
        {
            return;
        }

        TimeLeft.Value -= Time.deltaTime;

        if (TimeLeft.Value < 0)
        {
            CountdownFinishedEvent?.Invoke();
        }
    }
}
