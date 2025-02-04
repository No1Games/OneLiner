using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GemShop : MenuBase
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button freeTierBtn;
    [SerializeField] private Button firstTierBtn;
    [SerializeField] private Button secondTierBtn;
    [SerializeField] private Button thirdTierBtn;
    [SerializeField] private Button maxTierBtn;

    [SerializeField] private AdsWatchingCounter watchingCounter;

    public override MenuName Menu => MenuName.GemShop;

    private void Start()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        backBtn.onClick.AddListener(OnClick_BackButton);
        
        firstTierBtn.onClick.AddListener(() => GetGems(50));
        secondTierBtn.onClick.AddListener(() => GetGems(300));
        thirdTierBtn.onClick.AddListener(() => GetGems(1000));
        maxTierBtn.onClick.AddListener(() => GetGems(2500));

        freeTierBtn.onClick.AddListener(() => GetFreeGems());
        AdsManager.Instance.OnGemsShopRewardAllowed += () => GemManager.Instance.AddGems(5);

    }

    private void GetGems(int gems)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click); //remove when added in game purchase
        GemManager.Instance.AddGems(gems);
    }

    private void GetFreeGems()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (watchingCounter.CanWatchAd())
        {
            
            AdsManager.Instance.ShowRewardedAds(AdsPlaces.GemsShop);
        }
    }



    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenuToPrevious();
    }


}
