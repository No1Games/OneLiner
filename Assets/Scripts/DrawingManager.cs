using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    public GameObject linePrefab;
    private GameObject currentLine;
    private LineRenderer lineRenderer;

    private bool firstLineDone = false;
    private List<GameObject> lines = new List<GameObject>();
<<<<<<< HEAD

=======
>>>>>>> parent of aa20503 (Score and some game ending)
    [SerializeField] private float minDistance = 0.1f; // Відстань для перевірки, чи точка близька до лінії
    [SerializeField] private float minLength = 2f; // Відстань для перевірки, чи достатньо довга лінія
    [SerializeField] private float secondPointAngle = 10f; // Кут для перевірки чи не йде друга точка вздовж лінії з якої почалась

    private GameObject lineToTrack;

    #region Input
    TouchControls touchController;
    #endregion

    private void Awake()
    {
        touchController = new TouchControls();
        touchController.Touch.Tuch.started += ctx => StartDrawing();
        touchController.Touch.Tuch.performed += ctx => SecondPointUpdate();
        touchController.Touch.Tuch.canceled += ctx => FinishDrawing();
    }

    private void OnEnable()
    {
        touchController.Enable();
    }

    private void OnDisable()
    {
        touchController.Disable();
    }

    void Update()
    {
<<<<<<< HEAD
        // Можна використовувати для інших потреб, якщо буде потрібно
=======
        GenerateLine();
>>>>>>> parent of aa20503 (Score and some game ending)
    }

    // Перевіряємо, чи курсор знаходиться над UI елементом, який є кнопкою
    bool IsPointerOverButton()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void StartDrawing()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
        if (!IsPointerOverButton() && FirstPointPositionCheck(mousePos))
        {
            // Створення нової лінії
            currentLine = Instantiate(linePrefab);
            lineRenderer = currentLine.GetComponent<LineRenderer>();

            // Ініціалізація першої точки
            lineRenderer.positionCount = 2; // Встановлюємо кількість точок
            lineRenderer.SetPosition(0, mousePos);
            lineRenderer.SetPosition(1, mousePos);
            firstLineDone = true; // Першу лінію закінчено
        }
    }

    private void SecondPointUpdate()
    {
        if (lineRenderer != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
            lineRenderer.SetPosition(1, mousePos);

            // Зміна кольору лінії в залежності від довжини
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
        }
    }

    private void FinishDrawing()
    {
        if (lineRenderer != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
            lineRenderer.SetPosition(1, mousePos);

            if (Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < minLength || !SecondPointDistanceCheck())
            {
                Destroy(currentLine);
            }
            else
            {
                lines.Add(currentLine);
                currentLine = null; // Скидаємо наявну лінію
            }
        }
    }

    public void RemoveLastLine()
    {
        if (lines.Count > 0)
        {
            Destroy(lines[lines.Count - 1]);
            lines.RemoveAt(lines.Count - 1);
        }
    }

    private bool FirstPointPositionCheck(Vector3 point)
    {
        if (!firstLineDone) return true;

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
                    return true;
                }
            }
        }
        return false;
    }

    private bool SecondPointDistanceCheck()
    {
        if (!firstLineDone) return true;

        LineRenderer oldLine = lineToTrack.GetComponent<LineRenderer>();
        LineRenderer newLine = lineRenderer;

        Vector3 vectorToStartOldLine = oldLine.GetPosition(0) - newLine.GetPosition(0);
        Vector3 vectorToEndOldLine = oldLine.GetPosition(1) - newLine.GetPosition(0);
        Vector3 newLineVector = newLine.GetPosition(1) - newLine.GetPosition(0);

        float angleToStart = Vector3.Angle(newLineVector, vectorToStartOldLine);
        float angleToEnd = Vector3.Angle(newLineVector, vectorToEndOldLine);

        return angleToEnd > secondPointAngle && angleToStart > secondPointAngle;
    }

    // Відстань від точки до відрізка
    float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        Vector3 pointToStart = point - lineStart;
        float lineLengthSquared = lineDir.sqrMagnitude;
        float t = Mathf.Clamp01(Vector3.Dot(pointToStart, lineDir) / lineLengthSquared);
        Vector3 closestPoint = lineStart + t * lineDir;
        return Vector3.Distance(point, closestPoint);
    }
}
