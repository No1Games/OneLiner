using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string _iOSAdUnitId = "Interstitial_iOS";
    private string _adUnitId = null; // Цей ідентифікатор залишиться null для непідтримуваних платформ

    private bool _adLoaded = false;

    void Awake()
    {
        // Визначаємо Ad Unit ID для відповідної платформи:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        LoadAd();
    }

    // Метод для завантаження реклами
    public void LoadAd()
    {
        if (!_adLoaded)
        {
            Advertisement.Load(_adUnitId, this);
        }
    }

    // Метод для показу реклами
    public void ShowAd()
    {
        if (_adLoaded)
        {
            Advertisement.Show(_adUnitId, this);
            _adLoaded = false; // Скидаємо статус після показу
        }
        else
        {
            Debug.Log("Interstitial ad is not ready yet.");
        }

        LoadAd(); // Завантажуємо наступну рекламу після показу
    }

    // Callback після завершення показу реклами
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Interstitial Ad Completed");
            // Тут можна додати логіку, наприклад, щоб продовжити гру
        }
    }

    // Callback при успішному завантаженні реклами
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId.Equals(_adUnitId))
        {
            _adLoaded = true;
            Debug.Log("Interstitial ad loaded successfully.");
        }
    }

    // Callback при невдалій спробі завантажити рекламу
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Failed to load Interstitial ad for {adUnitId}: {error.ToString()} - {message}");
    }

    // Callback при невдачі під час показу реклами
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Interstitial ad {adUnitId}: {error.ToString()} - {message}");
    }

    // Інші callback-и, які можна залишити порожніми:
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
