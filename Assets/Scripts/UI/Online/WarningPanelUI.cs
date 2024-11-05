using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningPanelUI : MonoBehaviour
{
    private bool _isActive = false;

    [SerializeField] private NGODrawManager _drawingManager;

    [SerializeField] private TextMeshProUGUI _textTMP;
    [SerializeField] private Image _background;

    private Coroutine _warningCoroutine;

    private void Start()
    {
        _drawingManager.OnLineUnavailable += WarningActivate;

        _textTMP.gameObject.SetActive(false);
        _background.enabled = false;
    }

    private IEnumerator PushWarning(string message)
    {
        _isActive = true;
        _textTMP.text = message;
        _textTMP.gameObject.SetActive(true);
        _background.enabled = true;

        yield return new WaitForSeconds(2f);

        _textTMP.gameObject.SetActive(false);
        _background.enabled = false;
        _isActive = false;
    }

    public void WarningActivate(string message)
    {
        if (_isActive)
        {
            StopCoroutine(_warningCoroutine);
            _isActive = false;
        }

        _warningCoroutine = StartCoroutine(PushWarning(message));
    }
}
