using System.Collections.Generic;
using UnityEngine;

public class StandardDrawingValidator : IDrawingValidator
{
    private readonly float minLength;
    private readonly float minDistance;    
    private readonly List<GameObject> existingLines;
    private string currentProblem; 


    public string GetProblemText()
    {
        return currentProblem;
    }
    public StandardDrawingValidator(List<GameObject> existingLines, float minLength, float minDistance)
    {
        this.existingLines = existingLines;
        this.minLength = minLength;
        this.minDistance = minDistance;
        
    }

    public bool CanStartDrawing(ref Vector3 startPos)
    {
        currentProblem = "InGame_Warning_WrongStart";
        if (existingLines.Count == 0)
        {
            return true;
        }
        if (FindNearestPointOnLines(startPos, out Vector3 pointOnLine))
        {
            startPos = pointOnLine;
            return true;
        }
        
        return false; 
    }


    private bool FindNearestPointOnLines(Vector3 point, out Vector3 nearestPoint)
    {
        nearestPoint = point;        
        bool found = false;

        foreach (GameObject lineObj in existingLines)
        {
            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            if (line == null || line.positionCount < 2)
                continue;

            Vector3 startPoint = line.GetPosition(0);
            Vector3 endPoint = line.GetPosition(1);
            Vector3 closest = GetClosestPointOnSegment(point, startPoint, endPoint);
            float distance = Vector3.Distance(point, closest);

            if (distance < minDistance)
            {
                
                nearestPoint = closest;
                found = true;
            }
        }

        return found;
    }

    
    private Vector3 GetClosestPointOnSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLengthSquared = lineDir.sqrMagnitude;
        if (lineLengthSquared == 0)
            return lineStart;

        float t = Mathf.Clamp01(Vector3.Dot(point - lineStart, lineDir) / lineLengthSquared);
        return lineStart + t * lineDir;
    }

    

    
    public bool IsValidLine(Vector3 start, Vector3 end)
    {
        if (!IsLineLongEnough(start, end))
        {
            currentProblem = "InGame_Warning_TooShort";
            return false;
        }
        else if( !IsLineOnlyOneOnItVector(start, end))
        {
            currentProblem = "InGame_Warning_WrongVector";
            return false;
        }
        return true;

        
    }

    private bool IsLineLongEnough(Vector3 start, Vector3 end)
    {
        float totalLength =  Vector3.Distance(start, end);
        return totalLength >= minLength;
    }
    private bool IsLineOnlyOneOnItVector(Vector3 start, Vector3 end)
    {
        if (existingLines.Count == 0)
        {
            return true; 
        }
        foreach (GameObject lineObj in existingLines)
        {
            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            if (line == null || line.positionCount < 2)
                continue;

            Vector3 a = line.GetPosition(0);
            Vector3 b = line.GetPosition(1);

            
            Vector3 closest = GetClosestPointOnSegment(start, a, b);
            float distToStart = Vector3.Distance(start, closest);
            if (distToStart > minDistance)
                continue; 

            
            Vector3 dir = (b - a).normalized;
            Vector3 toEnd = (end - start).normalized;

            // Якщо перехресний добуток дуже малий → вектори майже паралельні
            float cross = Vector3.Cross(dir, toEnd).magnitude;
            if (cross < 0.1f) 
            {
                return false;
            }
        }

        
        return true;
    }
    
}
