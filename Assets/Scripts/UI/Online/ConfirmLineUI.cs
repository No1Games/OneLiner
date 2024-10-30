using UnityEngine;
using UnityEngine.UI;

public class ConfirmLineUI : MonoBehaviour
{
    [SerializeField] private DrawingManager _drawingManager;

    [SerializeField] private GameObject _panelGO;

    [SerializeField] private Button _removeBtn;

    private void Start()
    {
        _drawingManager.OnDrawingComplete += ConfirmTurnActivate;

        _removeBtn.onClick.AddListener(OnClick_RemoveButton);
    }

    private void ConfirmTurnActivate()
    {
        _drawingManager.DrawingAllowed = false;
        _panelGO.SetActive(true);
    }

    private void OnClick_RemoveButton()
    {
        _panelGO.SetActive(false);
        _drawingManager.RemoveLastLine();
        _drawingManager.DrawingAllowed = true;
    }
}
