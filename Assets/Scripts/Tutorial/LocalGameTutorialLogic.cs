using NUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocalGameTutorialLogic : BasicTutorialLogic
{

    [SerializeField] private UIManager uIManager;
    [SerializeField] private DrawingManager drawingManager;
    [SerializeField] private DrawingUpdate drawingUpdate;
    [SerializeField] private TutorialManager tutorialManager;

    [SerializeField] private List<Line> autoLinesPositions;



    public override void PerformActionForPage(TutorialPage page)
    {


        int index = page.PageCode;


        switch (index)
        {

            

            case 3:
                //draw first line
                StartAutoDrawAndScreenshot(autoLinesPositions[0].start, autoLinesPositions[0].end);

                break;
            case 4:
                uIManager.OpenWordsMenu();                
                break;
            case 6:
                uIManager.CloseWordsMenu();
                break;
            case 7:
                // open drawing menu
                uIManager.OpenDrawingMenu();
                break;
            case 8:
                //allow drawing wih special settings its allowing drawing a started first position
                drawingManager.DrawingAllowed = true;
                drawingManager.OnTestDrawingComplete += CallNextStepAfterDrawing;
                break;
            case 11:
                //close drawing menu and confirm line
                drawingUpdate.TakeScreenshot();
                break;
            case 12:
                //draw other lines
                StartDrawSecondPart();

                break;
            case 13:
                uIManager.OnTutorialWordCheck += TutorialWordCheck;
                uIManager.OpenWordsMenu();

                break;
            case 16:
                
                break;
            
            case 18:
                //show wining screen
                break;

            case 22:
                //return to main menu + give a reward
            default:
                Debug.Log("Default tutorial action.");
                break;
        }



    }
    private void CallNextStepAfterDrawing()
    {
        // можна додати логіку перевірки лінії
        tutorialManager.GoToNextPage();
        uIManager.CallCheckUpMenu();

    }
    public void StartAutoDrawAndScreenshot(Vector2 start, Vector2 end)
    {
        StartCoroutine(AutoDrawRoutine(start, end));
    }

    private IEnumerator AutoDrawRoutine(Vector2 start, Vector2 end)
    {
        uIManager.OpenDrawingMenu();

        yield return new WaitForEndOfFrame();

        drawingManager.DrawAutoLine(start, end);

        yield return new WaitForEndOfFrame(); // дочекайся рендеру, щоб камера точно бачила лінію

        drawingUpdate.TakeScreenshot();
    }
    public void StartDrawSecondPart()
    {
        StartCoroutine(DrawSecondPart());
    }

    private IEnumerator DrawSecondPart()
    {
        uIManager.OpenDrawingMenu();

        yield return new WaitForEndOfFrame();

        drawingManager.DrawAutoLine(autoLinesPositions[1].start, autoLinesPositions[1].end);
        drawingManager.DrawAutoLine(autoLinesPositions[2].start, autoLinesPositions[2].end);
        drawingManager.DrawAutoLine(autoLinesPositions[3].start, autoLinesPositions[3].end);

        yield return new WaitForEndOfFrame(); // дочекайся рендеру, щоб камера точно бачила лінію

        drawingUpdate.TakeScreenshot();
    }
    
    private void TutorialWordCheck(bool result)
    {
        if (result)
        {
            tutorialManager.SkipToPage(18);
        }
        else
        {
            tutorialManager.GoToNextPage();
        }
    }


    [Serializable]
    public class Line
    {
        public Vector3 start; 
        public Vector3 end;

    }
}
