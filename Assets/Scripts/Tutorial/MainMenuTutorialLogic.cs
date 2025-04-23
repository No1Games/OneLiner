using UnityEngine;

public class MainMenuTutorialLogic : BasicTutorialLogic
{
    [SerializeField] AccountManager accountManager;
    [SerializeField] LocalGameSetupManager localGameSetupManager;
    public override void PerformActionForPage(TutorialPage page) 
    {
        
        
        int index = page.PageCode;


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
                IngameData.Instance.IsTutorialOn = true;
                localGameSetupManager.FinishTutorial();
                break;
            default:
                Debug.Log("Default tutorial action.");
                break;
        }

    }
}
