using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalSelector : MonoBehaviour
{
    private bool isActive = false;

    public void ChooseLocal(int index)
    {
        Debug.Log(index);
        if (isActive)
        {
            return;
        }
        StartCoroutine( SetLocal(index) );
    }
    IEnumerator SetLocal(int localIndex)
    {
        Debug.Log("Language changed to" + localIndex);
        isActive = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localIndex];
        isActive= false;
    }
}
