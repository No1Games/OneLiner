using UnityEngine;

using UnityEngine.Advertisements;
using System;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{

    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms

    private bool _adLoaded = false;

    public event Action OnAdsShowed;

    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif
        LoadAd();

    }

    // Call this public method when you want to get an ad ready to show.
    public void LoadAd()
    {
        //if (!_adLoaded)
        //{
        //    Advertisement.Load(_adUnitId, this);
        //}
        

        Advertisement.Load(_adUnitId, this);
    }



    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        

        Advertisement.Show(_adUnitId, this);

        //if (_adLoaded) // Перевіряємо, чи реклама готова
        //{
        //    Advertisement.Show(_adUnitId, this);
        //    _adLoaded = false; // Скидаємо статус після показу
        //}
        //else
        //{
        //    Debug.Log("Ad is not ready yet.");
        //}

        LoadAd();
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            OnAdsShowed?.Invoke();
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward.
        }
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");

    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        _adLoaded = true;
    }
}