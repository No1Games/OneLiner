using UnityEngine;

public class PlayerGamesTracker : MonoBehaviour
{
    private int gamesPlayed;
    [SerializeField] private int gamesBetweenAds;

    private string playedGameKey = "LocalGamesPlayed";

    private void Awake()
    {
        gamesPlayed = PlayerPrefs.GetInt(playedGameKey, 0);
    }

    public bool CanRunWithoutAds()
    {
        //add if player status is prem return true
        if(gamesPlayed < gamesBetweenAds)
        {
            gamesPlayed++;
            SaveData();
            return true;
        }
        
        gamesPlayed = 0;
        SaveData();
        return false;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(playedGameKey, gamesPlayed);
        
        PlayerPrefs.Save();
    }
}
