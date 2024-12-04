using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PremiumShop : MenuBase
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button tierOneBtn;
    [SerializeField] private Button tierTwoBtn;
    [SerializeField] private Button tierThreeBtn;
    [SerializeField] private Button tierMaxBtn;

    [SerializeField] private AccountManager accountManager;
    public override MenuName Menu => MenuName.PremiumShop;
    private void Start()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        backBtn.onClick.AddListener(OnClick_BackButton);
        tierOneBtn.onClick.AddListener(() => SetPremiumTime(1));
        tierTwoBtn.onClick.AddListener(() => SetPremiumTime(3));
        tierThreeBtn.onClick.AddListener(() => SetPremiumTime(5));
        tierMaxBtn.onClick.AddListener(() => SetPremiumTime(100));

    }

    private void SetPremiumTime(float time)
    {
        float premiumEndData = time;
        Debug.Log("Premium available until: " + premiumEndData);
        accountManager.SetAccountStatus(AccountStatus.Premium);
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }

    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainShop);
    }

}
