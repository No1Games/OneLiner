using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    public GameObject linePrefab;
    private GameObject currentLine;
    private LineRenderer lineRenderer;
    [SerializeField] private Camera drawingCam;
    private bool isDrawing = false;
    private bool firstLineDone = false;
    private bool drawingAllowed = false;
    public bool DrawingAllowed
    {
        set { drawingAllowed = value; }
    }

    public event Action OnDrawingComplete;
    public event Action<string> OnLineUnavailable;

    private List<GameObject> lines = new List<GameObject>();
    public int drawenLines;
    [SerializeField] private float minDistance = 0.5f; // Відстань для перевірки, чи точка близька до лінії
    [SerializeField] private float minLength = 2f; // Відстань для перевірки, чи достатньо довга лінія
    [SerializeField] private float secondPointAngle = 10f; // кут для перевірки чи не йде друга точка вздовж лінії з якої почалась

    void Update()
    {
        GenerateLine();
        drawenLines = lines.Count;
    }

    // Перевіряємо, чи курсор знаходиться над UI елементом, який є кнопкою
    bool IsPointerOverButton()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // Отримуємо об'єкт, на який наведений курсор
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            // Перевіряємо, чи є серед результатів кнопка
            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return true; // Якщо є кнопка, курсор над нею
                }
            }
        }
        return false; // Якщо немає кнопки, можна малювати
    }
    private void GenerateLine()
    {
        IDrawingValidator validator = new StandardDrawingValidator(lines, minLength, minDistance);

        string lineTooShort = "InGame_Warning_TooShort";
        string lineWrongVector = "InGame_Warning_WrongVector";
        string lineWrongStart = "InGame_Warning_WrongStart";

        if (drawingCam.gameObject.activeInHierarchy)
        {


            // Отримуємо позицію миші або дотику
            Vector3 mousePos = drawingCam.ScreenToWorldPoint(Input.mousePosition);
            float distanceFromCamera = 15f;
            mousePos.z = drawingCam.transform.position.z + distanceFromCamera;

            bool touchBegan = false;
            bool touchMoved = false;
            bool touchEnded = false;

            // Перевірка на дотик
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                mousePos = drawingCam.ScreenToWorldPoint(touch.position);
                mousePos.z = drawingCam.transform.position.z + distanceFromCamera;

                touchBegan = touch.phase == TouchPhase.Began;
                touchMoved = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                touchEnded = touch.phase == TouchPhase.Ended;
            }

            // Малювання дозволено та вказівник не знаходиться над кнопкою
            if (drawingAllowed && !IsPointerOverButton())
            {
                // Початок малювання: або натискання миші, або початок дотику
                if (Input.GetMouseButtonDown(0) || touchBegan)
                {
                    if (validator.CanStartDrawing(ref mousePos))
                    {

                        currentLine = Instantiate(linePrefab);
                        lineRenderer = currentLine.GetComponent<LineRenderer>();

                        if (lineRenderer == null)
                        {
                            Debug.LogError("LineRenderer component not found on linePrefab!");
                            return;
                        }

                        lineRenderer.SetPosition(0, mousePos);
                        lineRenderer.SetPosition(1, mousePos);
                        isDrawing = true; // Малювання активне
                    }
                    else
                    {
                        OnLineUnavailable.Invoke(lineWrongStart);
                    }
                }

                // Малювання активне
                if (isDrawing)
                {
                    // Перевірка на рух миші або дотик
                    if (Input.GetMouseButton(0) || touchMoved)
                    {
                        if (validator.IsValidLine(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)))
                        {
                            lineRenderer.startColor = Color.black;
                            lineRenderer.endColor = Color.black;

                        }
                        else
                        {
                            lineRenderer.startColor = Color.red;
                            lineRenderer.endColor = Color.red;

                        }

                        lineRenderer.SetPosition(1, mousePos);
                    }

                    // Завершення малювання: або відпускання миші, або завершення дотику
                    if (Input.GetMouseButtonUp(0) || touchEnded)
                    {
                        isDrawing = false; // Завершити малювання
                        if (Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < minLength)
                        {
                            OnLineUnavailable.Invoke(lineTooShort);
                            Destroy(currentLine);
                        }
                        else if (!validator.IsValidLine(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)))
                        {
                            OnLineUnavailable.Invoke(lineWrongVector);
                            Destroy(currentLine);
                        }
                        else
                        {
                            lines.Add(currentLine);
                            if (!firstLineDone)
                            {
                                firstLineDone = true;
                            }
                            OnDrawingComplete?.Invoke();
                        }
                    }
                }
            }
        }
    }


    public void RemoveLastLine()
    {
        if (lines.Count > 0)
        {
            Destroy(lines[lines.Count - 1]);
            lines.RemoveAt(lines.Count - 1);
            if (lines.Count == 0)
            {
                firstLineDone = false;

            }
        }

    }

}
