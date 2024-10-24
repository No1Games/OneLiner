using TMPro;
using UnityEngine;

public class RuntimeLogger : ILogger
{
    private TextMeshProUGUI _logField;
    private const string _fieldTag = "LogField";

    public void Log(string message)
    {
        if (_logField == null)
        {
            GameObject logFieldGO = GameObject.FindGameObjectWithTag(_fieldTag);
            if (logFieldGO != null)
            {
                _logField = logFieldGO.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.Log("Faild to find log field");
            }
        }

        if (_logField != null)
        {
            string newLogs = $"{message}\n{_logField.text}";
            _logField.text = newLogs;
        }

        Debug.Log(message);
    }
}
