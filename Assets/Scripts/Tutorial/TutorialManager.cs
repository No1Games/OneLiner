using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialPage> tutorialPages;
    
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

    private void Start()
    {
        ShowPage(0);
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
    

    private void EndTutorial()
    {
        Debug.Log("Tutorial completed!");
        // Можна зберегти прогрес, наприклад:
        // PlayerPrefs.SetInt("TutorialCompleted", 1);
    }


}
