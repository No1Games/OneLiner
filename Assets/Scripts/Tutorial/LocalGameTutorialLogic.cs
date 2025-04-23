using System.Collections.Generic;
using UnityEngine;

public class LocalGameTutorialLogic : BasicTutorialLogic
{

    



    public override void PerformActionForPage(TutorialPage page)
    {


        int index = page.PageCode;


        switch (index)
        {
            case 0:


                break;
            case 1:
                
                break;
            case 2:
                MainMenuManager.Instance.ChangeMenu(MenuName.LocalSetup);
                break;
            
            default:
                Debug.Log("Default tutorial action.");
                break;
        }

    }
}
