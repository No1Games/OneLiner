using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    [SerializeField] AdsInitializer adsInitializer;
    [SerializeField] RewardedAds rewardedAds;
    [SerializeField] InterstitialAds interstitialAds;

    public static AdsManager Instance { get; private set; }
    private AdsPlaces adsPlace;

    public event Action OnGemsShopRewardAllowed;
    public event Action OnDoubleExpRewardAllowed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
        Init();
    }

    private void Init()
    {
        rewardedAds.OnAdsShowed += AllowReward;
        
    }
    private void AllowReward()
    {
        switch (adsPlace)
        {
            case AdsPlaces.GemsShop:
                OnGemsShopRewardAllowed?.Invoke();
                break;
            case AdsPlaces.BoostExp:
                OnDoubleExpRewardAllowed?.Invoke();
                break;
        }

    }

    public void ShowRewardedAds(AdsPlaces _adsPlace)
    {
        adsPlace = _adsPlace;
        rewardedAds.ShowAd();

    }
    public void ShowInterstitialAds()
    {
        interstitialAds.ShowAd();
    }
    
}
public enum AdsPlaces
{
    GemsShop,
    BoostReward,
    BoostExp
}