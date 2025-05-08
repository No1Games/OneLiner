using UnityEngine;

public class MainMenuTutorialLogic : BasicTutorialLogic
{

    [SerializeField] LocalGameSetupManager localGameSetupManager;
    [SerializeField] LGS_TimerController timerController;
    public override void PerformActionForPage(TutorialPage page) 
    {
        
        
        int index = page.PageCode;


        switch (index)
        {
            case 0:


                break;
            case 1:
                MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);
                IngameData.Instance.IsTutorialOn = true;
                break;
            case 2:
                MainMenuManager.Instance.ChangeMenu(MenuName.LocalSetup);
                timerController.TutorialOffTimer();
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
}
