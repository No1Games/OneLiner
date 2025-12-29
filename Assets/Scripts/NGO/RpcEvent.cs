public class RpcEvent
{
    public RpcEventType Type;
    public object Payload;

    public RpcEvent(RpcEventType type, object payload = null)
    {
        Type = type;
        Payload = payload;
    }
}

public enum RpcEventType
{
    ClientReady,    // payload: client id
    WordsReady,     // payload: none
    TurnPass,       // payload: next player
    UserGuess,      // payload: word index
    WrongGuess,     // payload: button index to disable, new hearts count
    LineSpawned,    // payload: two vectors for start and end of the line
    GameOver        // payload: is win and score
}