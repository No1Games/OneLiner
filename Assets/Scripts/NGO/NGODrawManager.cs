using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NGODrawManager : MonoBehaviour
{
    [SerializeField] private NGOLine _linePrefab;

    [SerializeField] private Camera _drawingCamera;

    private List<NGOLine> _lines;
    private Queue<NGOLine> _linesPool;
    private NGOLine _currentLine;
    private NGOLine _startLine;

    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _minLength = 2f;
    [SerializeField] private float _secondPointAngle = 10f;
    [SerializeField] private float _drawingDepth = 15f;

    private bool _isDrawing = false;
    private bool _isDrawAllowed = false;
    public bool IsDrawAllowed { set { _isDrawAllowed = value; } }

    public event Action<string> OnLineUnavailable;
    public event Action<NGOLine> OnLineDrawn;
    public event Action<NGOLine> OnLineConfirmed;
    public event Action OnLineSpawned;

    private readonly string _lineMustStartMessage = "Лінія має починатись з іншої лінії";
    private readonly string _lineTooShortMessage = "Лінія занадто коротка";
    private readonly string _lineMustNotContinueAnotherLineMessage = "Лінія не має продовжувати напрямок іншої лінії";

    void Start()
    {
        _lines = new List<NGOLine>();
        _linesPool = new Queue<NGOLine>();
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
                _currentLine.gameObject.SetActive(false);
                _linesPool.Enqueue(_currentLine);
                _currentLine = null;

                OnLineUnavailable?.Invoke(_lineTooShortMessage);
            }
            else if (!SecondPointAngleCheck())
            {
                _currentLine.gameObject.SetActive(false);
                _linesPool.Enqueue(_currentLine);
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

                    if (_linesPool.Count > 0)
                    {
                        _currentLine = _linesPool.Dequeue();
                        _currentLine.gameObject.SetActive(true);
                    }
                    else
                    {
                        _currentLine = Instantiate(_linePrefab);
                    }

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
            foreach (NGOLine line in _lines)
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
        NGOLine lastLine = _lines[_lines.Count - 1];

        _linesPool.Enqueue(lastLine);
        lastLine.gameObject.SetActive(false);

        _lines.RemoveAt(_lines.Count - 1);
    }

    public void AddLine(NGOLine line)
    {
        _lines.Add(line);
        OnLineSpawned?.Invoke();
    }

    public void LineConfirmed()
    {
        OnLineConfirmed?.Invoke(_currentLine);
    }
}
