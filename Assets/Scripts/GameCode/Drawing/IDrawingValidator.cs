using UnityEngine;

public interface IDrawingValidator
{
    string GetProblemText();
    bool CanStartDrawing(ref Vector3 startPos);
    bool IsValidLine(Vector3 start, Vector3 end);


}
