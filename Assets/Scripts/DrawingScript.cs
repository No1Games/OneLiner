using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private float minLength = 2f; // ³������ ��� ��������, �� ��������� ����� ���
    [SerializeField] private float secondPointAngle = 10f; // ��� ��� �������� �� �� ��� ����� ����� ������ �� � ��� ��������

    private GameObject lineToTrack;


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
            if (!isFirstPointSet)
            {
                if (FirstPointDistanceCheck(mousePos))
                {
                    // ��������� ���� ��
                    currentLine = Instantiate(linePrefab);
                    lineRenderer = currentLine.GetComponent<LineRenderer>();

                    // ����������� ����� �����
                    lineRenderer.SetPosition(0, mousePos);
                    lineRenderer.SetPosition(1, mousePos);

                    isFirstPointSet = true;
                }
                else
                {
                    Camera.main.GetComponent<CameraControl>().ShackCamera();
                }
               
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

        if (Input.GetMouseButtonUp(0) && isFirstPointSet && isSecondPointSet)
        {
            // ���������� ��������� ���� ���������� ������ ����
           
            if (Vector3.Distance(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)) < minLength || !SecondPointDistanceCheck())
            {
                Camera.main.GetComponent<CameraControl>().ShackCamera();
                Destroy(currentLine);
            }
            else
            {
                
                if (!firstLineDone)
                {
                    firstLineDone = true;
                }
                lines.Add(currentLine);

            }
            isFirstPointSet = false;
            isSecondPointSet = false;

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
                        lineToTrack = lineObj;
                        return true; // ���� ����� ������� �� ������� ��, ���������� �������� ���� ���
                    }
                }
            }
        }
        return false; // ���� ����� �� ������� �� ����� ��, �� ���������� ������ ���� ���
    }


    private bool SecondPointDistanceCheck() // ������� ������� �������� �� ��� ����� �����, ���� ���� ��� ������� (������� �� �� ���� ����� � ���� �� �� �������)
    {
        if (!firstLineDone)
        {
            return true; // ���� �� ���� ���, ������ ���������� ������ ����
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
                        
                        return true; // ���� ����� ������� �� ��������� ��, �� ���������� �������� ���� ���
                }
                
            
        }
        return false; // ���� ����� �� ������� �� ��������� ��, ���������� ������ ���� ���
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
