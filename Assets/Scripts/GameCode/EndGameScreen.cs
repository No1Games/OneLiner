
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Components;


public class EndGameScreen : MonoBehaviour
{
    [SerializeField] private GameObject loseContent;
    [SerializeField] private GameObject winContent;
    [SerializeField] private Button menuBtn;
    [SerializeField] private Button againBtn;
    [SerializeField] private List<GameObject> stars;
    

    [SerializeField] private LocalizeStringEvent localizeStringEvent;
    [SerializeField] private PlayerGamesTracker gamesTracker;

    private void Start()
    {
        menuBtn.onClick.AddListener(BackToMainMenu);
        againBtn.onClick.AddListener(PlayAgain);
        AdsManager.Instance.OnInterstitialAddsShown += LoadLocalGame;
    }
    public void SetFinaleScreen(string word, bool teamWin, int score = 0)
    {
        AudioManager.Instance.PauseSoundInBack();
        localizeStringEvent.StringReference.TableEntryReference = word;

        if (!teamWin)
        {
            AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_Lose);
            loseContent.SetActive(true);
            winContent.SetActive(false);
        }
        else
        {
            AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_Victory);
            winContent.SetActive(true);
            loseContent.SetActive(false);
            if (score <= 600) 
            {
                stars[0].SetActive(true);

            }
            if (score > 600 && score <= 800)
            {
                stars[1].SetActive(true);

            }
            if (score > 800 && score < 1000)
            {
                stars[2].SetActive(true);

            }
            if (score >= 1000)
            {
                stars[3].SetActive(true);

            }

        }

    }

    public void BackToMainMenu()
    {
        AudioManager.Instance.ResumeSoundInBack();
        AudioManager.Instance.StopSoundInAdditional();
        IngameData.Instance.SetReturnedFromGame(true);
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        AudioManager.Instance.StopSoundInAdditional();
        if (gamesTracker.CanRunWithoutAds())
        {
            LoadLocalGame();
        }
        else
        {
            AdsManager.Instance.ShowInterstitialAds();
        }



    }
    private void LoadLocalGame()
    {
        AudioManager.Instance.ResumeSoundInBack();
        
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
