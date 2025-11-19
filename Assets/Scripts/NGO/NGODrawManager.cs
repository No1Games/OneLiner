using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NGODrawManager : MonoBehaviour
{
    [SerializeField] private Line _linePrefab;
    [SerializeField] private Camera _drawingCamera;
    [SerializeField] private GameObject _drawingScreen;

    private ObjectPool<Line> _linesPool;

    private List<Line> _lines;
    private Line _currentLine;
    private Line _startLine;

    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _minLength = 2f;
    [SerializeField] private float _secondPointAngle = 10f;
    [SerializeField] private float _drawingDepth = 15f;

    private bool _isDrawing = false;
    private bool _isDrawAllowed = false;
    public bool IsDrawAllowed { set { _isDrawAllowed = value; } }

    public event Action<string> OnLineUnavailable;
    public event Action<Line> OnLineDrawn;
    public event Action<Vector3, Vector3> OnLineConfirmed;
    public event Action OnLineSpawned;

    private readonly string _lineMustStartMessage = "Line must start on another line";
    private readonly string _lineTooShortMessage = "Line is too short";
    private readonly string _lineMustNotContinueAnotherLineMessage = "Line can't continue another line";

    void Start()
    {
        _lines = new List<Line>();

        _linesPool = new ObjectPool<Line>(_linePrefab, _drawingScreen.transform);
    }

    void Update()
    {
        if (_isDrawing)
        {
            UpdateLine();
        }
        else
        {
            TryStartDrawing();
        }
    }

    private void UpdateLine()
    {
        if (IsTouchEnded() || IsMouseUp())
        {
            _isDrawing = false;

            if (_currentLine.GetLength() < _minLength)
            {
                _linesPool.ReturnObject(_currentLine);

                _currentLine = null;

                OnLineUnavailable?.Invoke(_lineTooShortMessage);
            }
            else if (!SecondPointAngleCheck())
            {
                _linesPool.ReturnObject(_currentLine);

                _currentLine = null;

                OnLineUnavailable?.Invoke(_lineMustNotContinueAnotherLineMessage);
            }
            else
            {
                OnLineDrawn?.Invoke(_currentLine);
            }
        }
        else
        {
            _currentLine.SetLineEnd(GetPointerPosition());

            if (_currentLine.GetLength() < _minLength || !SecondPointAngleCheck())
            {
                _currentLine.SetLineColor(Color.red);
            }
            else
            {
                _currentLine.SetLineColor(Color.black);
            }
        }
    }

    private void TryStartDrawing()
    {
        if (IsTouchBegan() || IsMouseDown())
        {
            if (_isDrawAllowed && !IsPointerOverButton())
            {
                Vector3 point = GetPointerPosition();
                if (FirstPointDistanceCheck(point))
                {
                    _isDrawing = true;

                    _currentLine = _linesPool.GetObject();

                    _currentLine.SetLinePositions(point, point);
                }
                else
                {
                    OnLineUnavailable?.Invoke(_lineMustStartMessage);
                }
            }
        }
    }

    private Vector3 GetPointerPosition()
    {
        Vector3 result;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            result = _drawingCamera.ScreenToWorldPoint(touch.position);
        }
        else
        {
            result = _drawingCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        result.z = _drawingCamera.transform.position.z + _drawingDepth;

        return result;
    }

    private bool IsTouchBegan()
    {
        if (Input.touchCount == 0)
            return false;

        Touch touch = Input.GetTouch(0);

        return touch.phase == TouchPhase.Began;
    }

    private bool IsMouseDown()
    {
        return Input.GetMouseButtonDown(0);
    }

    private bool IsTouchEnded()
    {
        if (Input.touchCount == 0)
            return false;

        Touch touch = Input.GetTouch(0);

        return touch.phase == TouchPhase.Ended;
    }

    private bool IsMouseUp()
    {
        return Input.GetMouseButtonUp(0);
    }

    private bool IsPointerOverButton()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);

            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool FirstPointDistanceCheck(Vector3 point)
    {
        if (_lines.Count == 0)
        {
            return true;
        }
        else
        {
            foreach (Line line in _lines)
            {
                Vector3[] positions = line.GetPositions();

                if (DistancePointToLineSegment(point, positions[0], positions[1]) <= _minDistance)
                {
                    _startLine = line;
                    return true;
                }
            }
        }
        return false;
    }

    private bool SecondPointAngleCheck()
    {
        if (_lines.Count == 0)
        {
            return true;
        }
        else
        {
            Vector3[] startLinePoints = _startLine.GetPositions();
            Vector3[] currentLinePoints = _currentLine.GetPositions();

            Vector3 vectorToStartOldLine = startLinePoints[0] - currentLinePoints[0];
            Vector3 vectorToEndOldLine = startLinePoints[1] - currentLinePoints[0];
            Vector3 newLineVector = currentLinePoints[1] - currentLinePoints[0];

            float angleToStart = Vector3.Angle(newLineVector, vectorToStartOldLine);
            float angleToEnd = Vector3.Angle(newLineVector, vectorToEndOldLine);

            if (angleToEnd > _secondPointAngle && angleToStart > _secondPointAngle)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        Vector3 pointToStart = point - lineStart;
        float lineLengthSquared = lineDir.sqrMagnitude;
        float t = Mathf.Clamp01(Vector3.Dot(pointToStart, lineDir) / lineLengthSquared);
        Vector3 closestPoint = lineStart + t * lineDir;
        return Vector3.Distance(point, closestPoint);
    }

    public void RemoveLastLine()
    {
        _linesPool.ReturnObject(_currentLine);
    }

    // Method to spawn line that was drawn by other players
    public Line SpawnLine(Vector3 start, Vector3 end)
    {
        // Instantiate the line and set positions
        Line line = _linesPool.GetObject();

        line.SetLinePositions(start, end);

        AddLine(line);

        // OnLineSpawned?.Invoke();

        return line;
    }

    public void AddLine(Line line)
    {
        _lines.Add(line);
    }

    public void LineConfirmed()
    {
        Vector3 start = _currentLine.Start;
        Vector3 end = _currentLine.End;
        RemoveLastLine();
        OnLineConfirmed?.Invoke(start, end);
    }
}
