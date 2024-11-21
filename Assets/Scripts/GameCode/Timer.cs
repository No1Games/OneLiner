using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private float maxTime;
    private float currentTime;
    [SerializeField] private TMP_Text timeText;
    private bool switcher = false;
    private bool isPaused = false;

    private bool alarmIsRunning = false;

    public event Action OnTimerEnds;
    void Update()
    {
        if (switcher)
        {
            RunTimer();
            
        }
       

    }
    public void TimerStrat()
    {
        StopAlarm();
        currentTime = maxTime;
        switcher = true;
        isPaused = false;

    }

    private void TimerEnds()
    {
        
        switcher = false;
        OnTimerEnds?.Invoke();

    }
    private void RunTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            StopAlarm();
            TimerEnds();
        }
        
        int timeToShow = Mathf.FloorToInt(currentTime % 60);
        if (timeToShow <= 10 && timeToShow > 0)
        {
            StartAlarm();
        }
        timeText.text = timeToShow.ToString();

    }
    private void PauseTimer()
    {
        if (!isPaused)
        {
            switcher = false;
            isPaused = true;

        }

    }

    private void ContinueTimer()
    {
        if (isPaused)
        {
            switcher = true;
        }

    }

    private void StartAlarm()
    {
        if ( !alarmIsRunning)
        {
            alarmIsRunning = true;
            AudioManager.Instance.PlaySoundInAdditional(GameSounds.Timer_LastSeconds);
        }
    }
    private void StopAlarm()
    {
        AudioManager.Instance.StopSoundInAdditional();
        alarmIsRunning = false;
    }
}
