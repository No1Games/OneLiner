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

    private bool firstLineDone = false;
    private List<GameObject> lines = new List<GameObject>();

    [SerializeField] private float minDistance = 0.1f; // ³������ ��� ��������, �� ����� ������� �� ����
    [SerializeField] private float minLength = 2f; // ³������ ��� ��������, �� ��������� ����� ����
    [SerializeField] private float secondPointAngle = 10f; // ��� ��� �������� �� �� ��� ����� ����� ������ ���� � ��� ��������

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
        GenerateLine();
    }
    
    private bool isDrawing = false;
    private bool firstLineDone = false;
    private bool drawingAllowed = false;
    public bool DrawingAllowed
    {
        set { drawingAllowed = value; }
    }

    public event Action OnDrawingComplete;

    private List<GameObject> lines = new List<GameObject>();
    public int drawenLines;
    [SerializeField] private float minDistance = 0.1f; // ³������ ��� ��������, �� ����� ������� �� ����
    [SerializeField] private float minLength = 2f; // ³������ ��� ��������, �� ��������� ����� ����
    [SerializeField] private float secondPointAngle = 10f; // ��� ��� �������� �� �� ��� ����� ����� ������ ���� � ��� ��������

    private GameObject lineToTrack;

    


    
    void Update()
    {
        GenerateLine();
        drawenLines = lines.Count;

    }
    bool IsPointerOverButton()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ��������� ���� ����
            currentLine = Instantiate(linePrefab);
            lineRenderer = currentLine.GetComponent<LineRenderer>();

            // ������������ ����� �����
            lineRenderer.positionCount = 2; // ������������ ������� �����
            lineRenderer.SetPosition(0, mousePos);
            lineRenderer.SetPosition(1, mousePos);
            firstLineDone = true; // ����� ���� ��������
        }
    }

    private void SecondPointUpdate()
    {
        if (lineRenderer != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(touchController.Touch.TuchPosition.ReadValue<Vector2>());
            lineRenderer.SetPosition(1, mousePos);

            // ���� ������� ���� � ��������� �� �������
            if (Vector3.Distance(lineRenderer.GetPosition(0), mousePos) < minLength || !SecondPointDistanceCheck())
            // �������� ��'���, �� ���� ��������� ������
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            // ����������, �� � ����� ���������� ������
            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return true; // ���� � ������, ������ ��� ���
                }
            }
        }
        return false; // ���� ���� ������, ����� ��������
    }

    private void FinishDrawing()
    {
        if (lineRenderer != null)
    private void GenerateLine()
    {
        // �������� ������� ���� ��� ������
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f;

        bool touchBegan = false;
        bool touchMoved = false;
        bool touchEnded = false;

        // �������� �� �����
        if (Input.touchCount > 0)
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
                currentLine = null; // ������� ������ ����
            touchBegan = touch.phase == TouchPhase.Began;
            touchMoved = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            touchEnded = touch.phase == TouchPhase.Ended;
        }

        // ��������� ��������� �� �������� �� ����������� ��� �������
        if (drawingAllowed && !IsPointerOverButton())
        {
            // ������� ���������: ��� ���������� ����, ��� ������� ������
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
                    isDrawing = true; // ��������� �������
                }
                else
                {
                    Camera.main.GetComponent<CameraControl>().ShackCamera();
                }
            }

            // ��������� �������
            if (isDrawing)
            {
                // �������� �� ��� ���� ��� �����
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

                // ���������� ���������: ��� ���������� ����, ��� ���������� ������
                if (Input.GetMouseButtonUp(0) || touchEnded)
                {
                    isDrawing = false; // ��������� ���������
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
        Destroy(lines[lines.Count - 1]);
        lines.RemoveAt(lines.Count - 1);
        if (lines.Count == 0)
        {
            firstLineDone = false;

        }
    }

    private bool FirstPointDistanceCheck(Vector3 point)
    {
        if (!firstLineDone)
        {
            return true; // ���� �� ���� ����, ������ ���������� ������ ����
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
                        return true; // ���� ����� ������� �� ������� ����, ���������� �������� ���� ����
                    }
                }
            }
        }
        return false; // ���� ����� �� ������� �� ����� ����, �� ���������� ������ ���� ����
    }


    private bool SecondPointDistanceCheck()
    {
        if (!firstLineDone)
        {
            return true; // ���� �� ���� ����, ������ ���������� ������ ����
        }
        else
        {

            LineRenderer oldLine = lineToTrack.GetComponent<LineRenderer>();
            LineRenderer newLine = currentLine.GetComponent<LineRenderer>();

            // ��������� �������
            Vector3 vectorToStartOldLine = oldLine.GetPosition(0) - newLine.GetPosition(0);
            Vector3 vectorToEndOldLine = oldLine.GetPosition(1) - newLine.GetPosition(0);
            Vector3 newLineVector = newLine.GetPosition(1) - newLine.GetPosition(0);

            // ���������� ����
            float angleToStart = Vector3.Angle(newLineVector, vectorToStartOldLine);
            float angleToEnd = Vector3.Angle(newLineVector, vectorToEndOldLine);



            if (angleToEnd > secondPointAngle && angleToStart > secondPointAngle)
            {

                return true; // ���� ����� ������� �� ��������� ����, �� ���������� �������� ���� ����
            }


        }
        return false; // ���� ����� �� ������� �� ��������� ����, ���������� ������ ���� ����
    }

    // ³������ �� ����� �� ������
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
