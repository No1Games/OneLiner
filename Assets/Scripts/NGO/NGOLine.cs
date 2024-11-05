using UnityEngine;

public class NGOLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _renderer;

    public void SetLinePositions(Vector3 first, Vector3 second)
    {
        _renderer.SetPosition(0, first);
        _renderer.SetPosition(1, second);
    }

    public void SetLineEnd(Vector3 end)
    {
        _renderer.SetPosition(1, end);
    }

    public void SetLineColor(Color color)
    {
        _renderer.startColor = color;
        _renderer.endColor = color;
    }

    public Vector3[] GetPositions()
    {
        return new Vector3[] { _renderer.GetPosition(0), _renderer.GetPosition(1) };
    }

    public float GetLength()
    {
        return Vector3.Distance(_renderer.GetPosition(0), _renderer.GetPosition(1));
    }
}
