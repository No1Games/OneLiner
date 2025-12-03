using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
    [SerializeField] private Button _drawButton;

    private void Start()
    {
        _drawButton.interactable = false;
    }

    public void UpdateUI(bool isInteractable)
    {
        _drawButton.interactable = isInteractable;
    }
}
