using UnityEngine;

public class BGImageController : MonoBehaviour
{
    [SerializeField] private GameObject _bgWhite;
    [SerializeField] private GameObject _bgBlue;

    public void SetBackground(BGType type)
    {
        _bgWhite.SetActive(type == BGType.White);
        _bgBlue.SetActive(type == BGType.Blue);
    }
}

public enum BGType
{
    White, Blue
}
