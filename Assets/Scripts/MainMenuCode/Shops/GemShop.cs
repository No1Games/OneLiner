using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;

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
        
        firstTierBtn.onClick.AddListener(PlayBtnSound);
        secondTierBtn.onClick.AddListener(PlayBtnSound);
        thirdTierBtn.onClick.AddListener(PlayBtnSound);
        maxTierBtn.onClick.AddListener(PlayBtnSound);

        freeTierBtn.onClick.AddListener(() => GetFreeGems());
        freeTierBtn.onClick.AddListener(PlayBtnSound);
        AdsManager.Instance.OnGemsShopRewardAllowed += () => GemManager.Instance.AddGems(5);

       

    }

    private void PlayBtnSound()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click); 
        
    }

    private void GetFreeGems()
    {
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
