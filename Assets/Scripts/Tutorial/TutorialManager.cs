using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialPage> tutorialPages;
    [SerializeField] private Button skipBtn;

    private int currentIndex = 0;

    
    [SerializeField] private BasicTutorialLogic tutorialLogic;
    

    private void Awake()
    {
        // Підписуємо кнопки один раз на старті
        foreach (var page in tutorialPages)
        {
            if (page.TutorialButton != null)
            {
                page.TutorialButton.onClick.RemoveAllListeners();
                page.TutorialButton.onClick.AddListener(() => OnTutorialButtonClicked(page));
            }
        }
    }

    private void OnEnable()
    {
        ShowPage(0);
        skipBtn.gameObject.SetActive(true);
        
        skipBtn.onClick.AddListener(OnSkipButtonClick);
    }
    private void OnDisable()
    {
        skipBtn.onClick.RemoveAllListeners();
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
    }

    private void OnTutorialButtonClicked(TutorialPage page)
    {
        // Тут основна логіка для цієї сторінки
        Debug.Log($"[Tutorial] Button clicked on page {tutorialPages.IndexOf(page)}");
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        tutorialLogic.PerformActionForPage(page);
        GoToNextPage();
    }
   
    

    public void GoToNextPage()
    {
        currentIndex++;
        if (currentIndex < tutorialPages.Count)
        {
            ShowPage(currentIndex);
        }
        else
        {
            EndTutorial();
        }
    }
    public void SkipToPage(int index)
    {
        ShowPage(index);
    }
    

    private void EndTutorial()
    {
        Debug.Log("Tutorial completed!");
        // Можна зберегти прогрес, наприклад:
        // PlayerPrefs.SetInt("TutorialCompleted", 1);
    }
    private void OnSkipButtonClick()
    {
        AccountManager.Instance.CurrentAccountData.tutorialStatus = TutorialStatus.Skipped;
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].gameObject.SetActive(false);
            skipBtn.gameObject.SetActive(false);
        }

        
        tutorialLogic.AdditionalSkipActivity();

    }


}

public enum TutorialStatus
{
    None = 0,
    Skipped = 1,
    Completed = 2,
    OnNewRun = 3
}

