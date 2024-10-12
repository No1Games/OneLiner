using System;
using TMPro;
using UnityEngine;

public class RuntimeTMPLogger : ILogger
{
    private TextMeshProUGUI _logField;
    public void Log(string message)
    {
        if (_logField == null)
        {
            try
            {
                _logField = GameObject.FindGameObjectWithTag("LogField").GetComponent<TextMeshProUGUI>();

                string logs = _logField.text;

                string newLogs = $"{message}\n{logs}";

                _logField.text = newLogs;
            }
            catch (Exception e)
            {
                Debug.Log($"TMP LOGGER NOT FOUND {e.Message}");
                Debug.Log(message);
            }
        }

        Debug.Log(message);
    }
}
