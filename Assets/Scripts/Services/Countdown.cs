using UnityEngine;

/// <summary>
/// Runs the countdown to the in-game state. While the start of the countdown is synced via Relay, the countdown itself is handled locally,
/// since precise timing isn't necessary.
/// </summary>
[RequireComponent(typeof(CountdownUI))]
public class Countdown : MonoBehaviour
{
    CallbackValue<float> TimeLeft = new CallbackValue<float>();

    private CountdownUI _ui;
    private const int _countdownTime = 4;

    private OnlineController _gameManager;

    public void Start()
    {
        if (_ui == null)
        {
            _ui = GetComponent<CountdownUI>();
        }

        _gameManager = OnlineController.Instance;

        _gameManager.AllPlayersReadyEvent += StartCountDown;
        _gameManager.PlayerNotReadyEvent += CancelCountDown;

        TimeLeft.onChanged += _ui.OnTimeChanged;
        TimeLeft.Value = -1;

        _ui.Hide();
    }

    private void OnDestroy()
    {
        TimeLeft.onChanged -= _ui.OnTimeChanged;

        _gameManager.AllPlayersReadyEvent -= StartCountDown;
        _gameManager.PlayerNotReadyEvent -= CancelCountDown;
    }

    public void StartCountDown()
    {
        TimeLeft.Value = _countdownTime;
        _ui.Show();
    }

    public void CancelCountDown()
    {
        TimeLeft.Value = -1;
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
            _gameManager.OnCountdownFinished();
        }
    }
}
