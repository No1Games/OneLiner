using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class WarningPanelUI : MonoBehaviour
{
    private bool _isActive = false;

    private Dictionary<WarningType, string> _warningTexts = new Dictionary<WarningType, string>();

    private readonly string _lineMustStartKey = "InGame_Warning_WrongStart";
    private readonly string _lineTooShortKey = "InGame_Warning_TooShort";
    private readonly string _lineMustNotContinueAnotherLineKey = "InGame_Warning_WrongVector";

    [SerializeField] private LocalizeStringEvent _warningLSE;
    [SerializeField] private Image _warningIcon;

    private Coroutine _warningCoroutine;

    private void Awake()
    {
        _warningTexts.Add(WarningType.WrongStart, _lineMustStartKey);
        _warningTexts.Add(WarningType.TooShort, _lineTooShortKey);
        _warningTexts.Add(WarningType.WrongVector, _lineMustNotContinueAnotherLineKey);
    }

    private void Start()
    {
        _warningLSE.gameObject.SetActive(false);
        _warningIcon.gameObject.SetActive(false);
    }

    private IEnumerator PushWarning(string message)
    {
        _isActive = true;
        _warningLSE.StringReference.TableEntryReference = message;
        _warningLSE.gameObject.SetActive(true);
        _warningIcon.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        _warningLSE.gameObject.SetActive(false);
        _warningIcon.gameObject.SetActive(false);
        _isActive = false;
    }

    public void WarningActivate(WarningType type)
    {
        if (_isActive)
        {
            StopCoroutine(_warningCoroutine);
            _isActive = false;
        }

        //string message = string.Empty;

        //switch (type)
        //{
        //    case WarningType.MustStart: message = _lineMustStartMessage; break;
        //    case WarningType.TooShort: message = _lineTooShortMessage; break;
        //    case WarningType.MustNotContinue: message = _lineMustNotContinueAnotherLineMessage; break;
        //}

        _warningCoroutine = StartCoroutine(PushWarning(_warningTexts[type]));
    }
}