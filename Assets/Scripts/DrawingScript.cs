using System.Collections.Generic;
using UnityEngine;

public class LineDrawing : MonoBehaviour
{
    public GameObject linePrefab;
    private GameObject currentLine;
    private LineRenderer lineRenderer;
    private bool isFirstPointSet = false;
    private bool isSecondPointSet = false;
    private bool firstLineDone = false;

    private List<GameObject> lines = new List<GameObject>();
    [SerializeField] private float minDistance = 0.1f; // ³������ ��� ��������, �� ����� ������� �� ��

    void Update()
    {
        GenerateLine();
    }

    private void GenerateLine()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f; // ��������� Z ���������� ��� �� (������ �� ������, �� ���)

        if (Input.GetMouseButtonDown(0))
        {
            if (!isFirstPointSet && FirstPointDistanceCheck(mousePos))
            {
                // ��������� ���� ��
                currentLine = Instantiate(linePrefab);
                lineRenderer = currentLine.GetComponent<LineRenderer>();

                // ����������� ����� �����
                lineRenderer.SetPosition(0, mousePos);
                lineRenderer.SetPosition(1, mousePos);

                isFirstPointSet = true;
            }
            else if (isFirstPointSet && !isSecondPointSet)
            {
                // ����������� ����� �����
                lineRenderer.SetPosition(1, mousePos);
                isSecondPointSet = true;
            }
        }

        if (Input.GetMouseButton(0) && isFirstPointSet && isSecondPointSet)
        {
            // ��������� ����� ����� ���� ������ ���������
            lineRenderer.SetPosition(1, mousePos);
        }

        if (Input.GetMouseButtonUp(0) && isFirstPointSet && isSecondPointSet)
        {
            // ���������� ��������� ���� ���������� ������ ����
            isFirstPointSet = false;
            isSecondPointSet = false;
            if (!firstLineDone)
            {
                firstLineDone = true;
            }
            lines.Add(currentLine);
        }
    }

    private bool FirstPointDistanceCheck(Vector3 point)
    {
        if (!firstLineDone)
        {
            return true; // ���� �� ���� ���, ������ ���������� ������ ����
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
                        return true; // ���� ����� ������� �� ������� ��, ���������� �������� ���� ���
                    }
                }
            }
        }
        return false; // ���� ����� �� ������� �� ����� ��, �� ���������� ������ ���� ���
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
