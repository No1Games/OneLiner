using System.Collections.Generic;
using UnityEngine;

public class LocalGameTutorialLogic : BasicTutorialLogic
{

    [SerializeField] private UIManager uIManager;




    public override void PerformActionForPage(TutorialPage page)
    {


        int index = page.PageCode;


        switch (index)
        {
            case 3:
                //draw first line

                break;
            case 4:
                uIManager.OpenWordsMenu();
                break;
            case 6:
                uIManager.CloseWordsMenu();
                break;
            case 7:
                // open drawing menu
                break;
            case 10:
                //allow drawing wih special settings its allowing drawing a started first position
                break;
            case 11:
                // if correct line go next if no repit
                break;
            case 12:
                //close drawing menu and confirm line
                    break;
            case 13:
                //draw other lines
                break;
            case 14:
                //return to main menu and give a reward
            
            default:
                Debug.Log("Default tutorial action.");
                break;
        }

    }
}
