using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalSelector : MonoBehaviour
{
    private bool isActive;

    public void ChooseLocal(int index)
    {
        if (isActive)
        {
            return;
        }
        SetLocal(index);
    }
    IEnumerator SetLocal(int localIndex)
    {
        isActive = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localIndex];
        isActive= false;
    }
}
