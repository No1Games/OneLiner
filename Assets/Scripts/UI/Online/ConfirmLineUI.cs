using UnityEngine;
using UnityEngine.UI;

public class ConfirmLineUI : MonoBehaviour
{
    [SerializeField] private NGODrawManager _drawingManager;
    [SerializeField] private DrawingUpdate _drawingUpdate;

    [SerializeField] private GameObject _panelGO;

    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _removeBtn;

    private void Start()
    {
        _drawingManager.OnDrawingEnd += ConfirmTurnActivate;

        _removeBtn.onClick.AddListener(OnClick_RemoveButton);
        _confirmBtn.onClick.AddListener(OnClick_ConfirmButton);
    }

    private void ConfirmTurnActivate(NGOLine line)
    {
        _drawingManager.IsDrawAllowed = false;
        _panelGO.SetActive(true);
    }

    private void OnClick_RemoveButton()
    {
        _panelGO.SetActive(false);
        _drawingManager.RemoveLastLine();
        _drawingManager.IsDrawAllowed = true;
    }

    private void OnClick_ConfirmButton()
    {
        Debug.Log("User confirmed his line");

        _panelGO.SetActive(false);
        _drawingUpdate.TakeScreenshot();
    }
}
