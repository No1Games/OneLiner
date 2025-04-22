using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialPage> tutorialPages;
    
    private int currentIndex = 0;

    [SerializeField] AccountManager accountManager;
    [SerializeField] LocalGameSetupManager localGameSetupManager;
    

    private void Awake()
    {
        // ϳ������� ������ ���� ��� �� �����
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
        // ��� ������� ����� ��� ���� �������
        Debug.Log($"[Tutorial] Button clicked on page {tutorialPages.IndexOf(page)}");
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        PerformActionForPage(page);
        GoToNextPage();
    }

    private void PerformActionForPage(TutorialPage page)
    {
        // � ��������� �� ������� ����� ��������� ���� �����
        int index = tutorialPages.IndexOf(page);
        
        
        switch (index)
        {
            case 0:
                

                break;
            case 1:
                MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);
                break;
            case 2:
                MainMenuManager.Instance.ChangeMenu(MenuName.LocalSetup);
                break;
            case 7:
                localGameSetupManager.AddPlayerForTutorial();
                break;
            case 9:
                localGameSetupManager.FinishTutorial();
                break;
            default:
                Debug.Log("Default tutorial action.");
                break;
        }

    }

    private void GoToNextPage()
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
        // ����� �������� �������, ���������:
        // PlayerPrefs.SetInt("TutorialCompleted", 1);
    }


}
