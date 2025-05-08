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
        
        if(AccountManager.Instance.CurrentAccountData.accountStatus == AccountStatus.Premium || IngameData.Instance.IsTutorialOn)
        {
            return true;
        }

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
