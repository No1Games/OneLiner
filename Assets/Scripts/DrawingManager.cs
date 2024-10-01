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

    
    private bool isDrawing = false;
    private bool firstLineDone = false;
    private bool drawingAllowed = false;
    public bool DrawingAllowed
    {
        set { drawingAllowed = value; }
    }

    public event Action OnDrawingComplete;


    private bool firstLineDone = false;
    private List<GameObject> lines = new List<GameObject>();


    [SerializeField] private float minDistance = 0.1f; // Â³äñòàíü äëÿ ïåðåâ³ðêè, ÷è òî÷êà áëèçüêà äî ë³í³¿
    [SerializeField] private float minLength = 2f; // Â³äñòàíü äëÿ ïåðåâ³ðêè, ÷è äîñòàòíüî äîâãà ë³í³ÿ
    [SerializeField] private float secondPointAngle = 10f; // Êóò äëÿ ïåðåâ³ðêè ÷è íå éäå äðóãà òî÷êà âçäîâæ ë³í³¿ ç ÿêî¿ ïî÷àëàñü

    private GameObject lineToTrack;


    



    
    void Update()
    {

        GenerateLine();

    }


   
    bool IsPointerOverButton()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void StartDrawing()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
        if (!IsPointerOverButton() && FirstPointPositionCheck(mousePos))
        {
            // Ñòâîðåííÿ íîâî¿ ë³í³¿
            currentLine = Instantiate(linePrefab);
            lineRenderer = currentLine.GetComponent<LineRenderer>();

            // ²í³ö³àë³çàö³ÿ ïåðøî¿ òî÷êè
            lineRenderer.positionCount = 2; // Âñòàíîâëþºìî ê³ëüê³ñòü òî÷îê
            lineRenderer.SetPosition(0, mousePos);
            lineRenderer.SetPosition(1, mousePos);
            firstLineDone = true; // Ïåðøó ë³í³þ çàê³í÷åíî
        }
    }

    private void SecondPointUpdate()
    {
        if (lineRenderer != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
            lineRenderer.SetPosition(1, mousePos);

            // Çì³íà êîëüîðó ë³í³¿ â çàëåæíîñò³ â³ä äîâæèíè
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

    private void GenerateLine()
    {
        // Îòðèìóºìî ïîçèö³þ ìèø³ àáî äîòèêó
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f;

        bool touchBegan = false;
        bool touchMoved = false;
        bool touchEnded = false;

        // Ïåðåâ³ðêà íà äîòèê
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            mousePos = Camera.main.ScreenToWorldPoint(touch.position);
            mousePos.z = -1f;

            touchBegan = touch.phase == TouchPhase.Began;
            touchMoved = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            touchEnded = touch.phase == TouchPhase.Ended;
        }

        // Ìàëþâàííÿ äîçâîëåíî òà âêàç³âíèê íå çíàõîäèòüñÿ íàä êíîïêîþ
        if (drawingAllowed && !IsPointerOverButton())
        {
            // Ïî÷àòîê ìàëþâàííÿ: àáî íàòèñêàííÿ ìèø³, àáî ïî÷àòîê äîòèêó
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

                    lineRenderer.SetPosition(0, mousePos);
                    lineRenderer.SetPosition(1, mousePos);
                    isDrawing = true; // Ìàëþâàííÿ àêòèâíå
                }
                else
                {
                    Camera.main.GetComponent<CameraControl>().ShackCamera();
                }
            }

            // Ìàëþâàííÿ àêòèâíå
            if (isDrawing)
            {
                // Ïåðåâ³ðêà íà ðóõ ìèø³ àáî äîòèê
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

                // Çàâåðøåííÿ ìàëþâàííÿ: àáî â³äïóñêàííÿ ìèø³, àáî çàâåðøåííÿ äîòèêó
                if (Input.GetMouseButtonUp(0) || touchEnded)
                {
                    isDrawing = false; // Çàâåðøèòè ìàëþâàííÿ
                    if (Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < minLength || !SecondPointDistanceCheck())
                    {
                        Camera.main.GetComponent<CameraControl>().ShackCamera();
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

    // Â³äñòàíü â³ä òî÷êè äî â³äð³çêà
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
