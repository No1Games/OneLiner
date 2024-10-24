using System.Collections.Generic;
using UnityEngine;

public class IngameData : MonoBehaviour
{

    public int playersAmount;

    public static IngameData Instance;

    public List<PlayerScript> players = new List<PlayerScript>();

    private GameModes selectedMode = GameModes.Coop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // «бер≥гаЇмо об'Їкт м≥ж сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameMode(GameModes mode)
    {
        selectedMode = mode;
    }

    public GameModes GetGameMode()
    {
        return selectedMode;
    }


}

public enum GameModes
{
    Coop,
    ReversCoop,


}
