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
    [SerializeField] private float minDistance = 0.1f; // Відстань для перевірки, чи точка близька до лінії
    [SerializeField] private float minLength = 2f; // Відстань для перевірки, чи достатньо довга лінія
    [SerializeField] private float secondPointAngle = 10f; // кут для перевірки чи не йде друга точка вздовж лінії з якої почалась

    private GameObject lineToTrack;


    void Update()
    {
        GenerateLine();
    }

    private void GenerateLine()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -1f; // Установка Z координати для лінії (ближче до камери, ніж фон)


        if (Input.GetMouseButtonDown(0))
        {
            if (!isFirstPointSet)
            {
                if (FirstPointDistanceCheck(mousePos))
                {
                    // Створення нової лінії
                    currentLine = Instantiate(linePrefab);
                    lineRenderer = currentLine.GetComponent<LineRenderer>();

                    // Ініціалізація першої точки
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
                // Ініціалізація другої точки
                lineRenderer.SetPosition(1, mousePos);
                isSecondPointSet = true;
            }
        }

        if (Input.GetMouseButton(0) && isFirstPointSet && isSecondPointSet)
        {
            // Оновлення другої точки поки кнопка натиснута
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
            // Завершення малювання після відпускання кнопки миші
           
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


    private bool SecondPointDistanceCheck() // можливо зробити перевірку як для першої точки, якщо нова лінія коротка (коротша ніж до будь якого з країв від її початку)
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
        return Vector3.Distance(point, closestPoint);
    }
}
