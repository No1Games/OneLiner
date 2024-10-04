using UnityEngine;

public class DefaultLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
}
