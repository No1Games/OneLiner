using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string _iOSAdUnitId = "Interstitial_iOS";
    private string _adUnitId = null; // ��� ������������� ���������� null ��� �������������� ��������

    private bool _adLoaded = false;

    void Awake()
    {
        // ��������� Ad Unit ID ��� �������� ���������:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        LoadAd();
    }

    // ����� ��� ������������ �������
    public void LoadAd()
    {
        if (!_adLoaded)
        {
            Advertisement.Load(_adUnitId, this);
        }
    }

    // ����� ��� ������ �������
    public void ShowAd()
    {
        if (_adLoaded)
        {
            Advertisement.Show(_adUnitId, this);
            _adLoaded = false; // ������� ������ ���� ������
        }
        else
        {
            Debug.Log("Interstitial ad is not ready yet.");
        }

        LoadAd(); // ����������� �������� ������� ���� ������
    }

    // Callback ���� ���������� ������ �������
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Interstitial Ad Completed");
            // ��� ����� ������ �����, ���������, ��� ���������� ���
        }
    }

    // Callback ��� �������� ����������� �������
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId.Equals(_adUnitId))
        {
            _adLoaded = true;
            Debug.Log("Interstitial ad loaded successfully.");
        }
    }

    // Callback ��� ������� ����� ����������� �������
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Failed to load Interstitial ad for {adUnitId}: {error.ToString()} - {message}");
    }

    // Callback ��� ������� �� ��� ������ �������
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Interstitial ad {adUnitId}: {error.ToString()} - {message}");
    }

    // ���� callback-�, �� ����� �������� ��������:
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
