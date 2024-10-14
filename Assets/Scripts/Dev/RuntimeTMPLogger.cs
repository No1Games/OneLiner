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
            }
            catch
            {
                return;
            }
        }

        string logs = _logField.text;

        string newLogs = $"{message}\n{logs}";

        _logField.text = newLogs;

        Debug.Log(message);
    }
}
