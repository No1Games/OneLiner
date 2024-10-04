using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    public GameObject linePrefab;
    private GameObject currentLine;
    private LineRenderer lineRenderer;
    
    private bool isDrawing = false;
    private bool firstLineDone = false;
    private bool drawingAllowed = false;
    public bool DrawingAllowed
    {
        set { drawingAllowed = value; }
    }

    public event Action OnDrawingComplete;
    public event Action <string> OnLineUnavailable;

    private List<GameObject> lines = new List<GameObject>();
    public int drawenLines;
    [SerializeField] private float minDistance = 0.5f; // Відстань для перевірки, чи точка близька до лінії
    [SerializeField] private float minLength = 2f; // Відстань для перевірки, чи достатньо довга лінія
    [SerializeField] private float secondPointAngle = 10f; // кут для перевірки чи не йде друга точка вздовж лінії з якої почалась

    private GameObject lineToTrack;


    Vector3 firstPointPosition;

    
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
        // Отримуємо позицію миші або дотику
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f;

        bool touchBegan = false;
        bool touchMoved = false;
        bool touchEnded = false;

        // Перевірка на дотик
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            mousePos = Camera.main.ScreenToWorldPoint(touch.position);
            mousePos.z = -1f;

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
                if (FirstPointDistanceCheck(mousePos))
                {
                    currentLine = Instantiate(linePrefab);
                    lineRenderer = currentLine.GetComponent<LineRenderer>();

                    if (lineRenderer == null)
                    {
                        Debug.LogError("LineRenderer component not found on linePrefab!");
                        return;
                    }

                    lineRenderer.SetPosition(0, firstPointPosition);
                    lineRenderer.SetPosition(1, firstPointPosition);
                    isDrawing = true; // Малювання активне
                }
                else
                {
                    OnLineUnavailable.Invoke("Лінія має починатись з іншої лінії");
                }
            }

            // Малювання активне
            if (isDrawing)
            {
                // Перевірка на рух миші або дотик
                if (Input.GetMouseButton(0) || touchMoved)
                {
                    if (Vector3.Distance(lineRenderer.GetPosition(0), mousePos) < minLength || !SecondPointDistanceCheck())
                    {
                        lineRenderer.startColor = Color.red;
                        lineRenderer.endColor = Color.red;
                    }
                    else
                    {
                        lineRenderer.startColor = Color.black;
                        lineRenderer.endColor = Color.black;
                    }

                    lineRenderer.SetPosition(1, mousePos);
                }

                // Завершення малювання: або відпускання миші, або завершення дотику
                if (Input.GetMouseButtonUp(0) || touchEnded)
                {
                    isDrawing = false; // Завершити малювання
                    if (Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < minLength)
                    {
                        OnLineUnavailable.Invoke("Лінія занадто коротка");
                        Destroy(currentLine);
                    }
                    else if (!SecondPointDistanceCheck())
                    {
                        OnLineUnavailable.Invoke("Лінія не має продовжувати напрямок іншої лінії");
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


    public void RemoveLastLine()
    {
        if(lines.Count > 0)
        {
            Destroy(lines[lines.Count - 1]);
            lines.RemoveAt(lines.Count - 1);
            if (lines.Count == 0)
            {
                firstLineDone = false;

            }
        }
        
    }

    private bool FirstPointDistanceCheck(Vector3 point)
    {
        if (!firstLineDone)
        {
            firstPointPosition = point;
            return true; // Якщо ще немає ліній, завжди дозволяємо почати нову
        }
        else
        {
            foreach (GameObject lineObj in lines)
            {
                LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
                if (lineRenderer != null && lineRenderer.positionCount == 2)
                {
                    Vector3 start = lineRenderer.GetPosition(0);
                    Vector3 end = lineRenderer.GetPosition(1);
                    if (DistancePointToLineSegment(point, start, end) < minDistance)
                    {
                        lineToTrack = lineObj;
                        return true; // Якщо точка близька до існуючої лінії, дозволяємо малювати нову лінію
                    }
                }
            }
        }
        return false; // Якщо точка не близька до жодної лінії, не дозволяємо почати нову лінію
    }


    private bool SecondPointDistanceCheck()
    {
        if (!firstLineDone)
        {
            return true; // Якщо ще немає ліній, завжди дозволяємо почати нову
        }
        else
        {

            LineRenderer oldLine = lineToTrack.GetComponent<LineRenderer>();
            LineRenderer newLine = currentLine.GetComponent<LineRenderer>();

            // Створюємо вектори
            Vector3 vectorToStartOldLine = oldLine.GetPosition(0) - newLine.GetPosition(0);
            Vector3 vectorToEndOldLine = oldLine.GetPosition(1) - newLine.GetPosition(0);
            Vector3 newLineVector = newLine.GetPosition(1) - newLine.GetPosition(0);

            // Обчислюємо кути
            float angleToStart = Vector3.Angle(newLineVector, vectorToStartOldLine);
            float angleToEnd = Vector3.Angle(newLineVector, vectorToEndOldLine);



            if (angleToEnd > secondPointAngle && angleToStart > secondPointAngle)
            {

                return true; // Якщо точка близька до початкової лінії, не дозволяємо малювати нову лінію
            }


        }
        return false; // Якщо точка не близька до початкової лінії, дозволяємо почати нову лінію
    }

    // Відстань від точки до відрізка
    float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {

        Vector3 lineDir = lineEnd - lineStart;
        Vector3 pointToStart = point - lineStart;
        float lineLengthSquared = lineDir.sqrMagnitude;
        float t = Mathf.Clamp01(Vector3.Dot(pointToStart, lineDir) / lineLengthSquared);
        Vector3 closestPoint = lineStart + t * lineDir;
        firstPointPosition = closestPoint;
        return Vector3.Distance(point, closestPoint);
    }
}
