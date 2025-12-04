using System;
using TMPro;
using UnityEngine;


public class AdsWatchingCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI adsCounterText;
    [SerializeField] private int maxAdsPerDay;
    [SerializeField] private string AdsKey;
    [SerializeField] private string DateKey;


    private int availableAds;
    private DateTime lastWatchedDate;
    

    private void Start()
    {
        LoadData();
        CheckDateReset();
        UpdateAdsCounterText();
    }

    public bool CanWatchAd()
    {
        CheckDateReset();
        if (availableAds > 0)
        {
            UseAd();
            return true;
        }
        return false;
    }

    private void UseAd()
    {
        availableAds--;
        UpdateAdsCounterText();
        SaveData();
    }

    private void CheckDateReset()
    {
        DateTime today = DateTime.Today;
        if (lastWatchedDate < today)
        {
            availableAds = maxAdsPerDay;
            lastWatchedDate = today;
            SaveData();
        }
    }

    private void LoadData()
    {
        availableAds = PlayerPrefs.GetInt(AdsKey, maxAdsPerDay);
        string savedDate = PlayerPrefs.GetString(DateKey, "");
        lastWatchedDate = string.IsNullOrEmpty(savedDate) ? DateTime.Today : DateTime.Parse(savedDate);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(AdsKey, availableAds);
        PlayerPrefs.SetString(DateKey, lastWatchedDate.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }

    private void UpdateAdsCounterText()
    {
        if (adsCounterText != null)
        {
            adsCounterText.text =  availableAds.ToString();
        }
    }
}
