using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PremiumShop : MenuBase
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button tierOneBtn;
    [SerializeField] private Button tierTwoBtn;
    [SerializeField] private Button tierThreeBtn;
    [SerializeField] private Button tierMaxBtn;

    [SerializeField] private Button tierOneInvoker;
    [SerializeField] private Button tierTwoInvoker;
    [SerializeField] private Button tierThreeInvoker;
    [SerializeField] private Button tierMaxInvoker;

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
        tierOneBtn.onClick.AddListener(() => OnTierOneClick());
        tierTwoBtn.onClick.AddListener(() => OnTierTwoClick());
        tierThreeBtn.onClick.AddListener(() => OnTierThreeClick());
        tierMaxBtn.onClick.AddListener(() => OnTierMaxClick());

    }

    private void OnTierOneClick()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (AccountManager.Instance.CurrentAccountData.hasLifetimeSubscription)
        {
            Debug.Log("You already have a subscription");
        }
        else
        {
            
            tierOneInvoker.onClick.Invoke();

        }
        
    }
    private void OnTierTwoClick()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (AccountManager.Instance.CurrentAccountData.hasLifetimeSubscription)
        {
            Debug.Log("You already have a subscription");
        }
        else
        {

            tierTwoInvoker.onClick.Invoke();

        }

    }
    private void OnTierThreeClick()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (AccountManager.Instance.CurrentAccountData.hasLifetimeSubscription)
        {
            Debug.Log("You already have a subscription");
        }
        else
        {

            tierThreeInvoker.onClick.Invoke();

        }

    }
    private void OnTierMaxClick()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (AccountManager.Instance.CurrentAccountData.hasLifetimeSubscription)
        {
            Debug.Log("You already have a subscription");
        }
        else
        {

            tierMaxInvoker.onClick.Invoke();

        }

    }

    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenuToPrevious();
    }

}
