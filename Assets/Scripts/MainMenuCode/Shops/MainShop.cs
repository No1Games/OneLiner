using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainShop : MenuBase
{
    [SerializeField] private Button gemShopBtn;
    [SerializeField] private Button premiumShopBtn;
    [SerializeField] private Button gemShopInfoBtn;
    [SerializeField] private Button premiumShopInfoBtn;
    [SerializeField] private Button backBtn;

    [SerializeField] private Button premiumBueBtn;
    [SerializeField] private Button gemBueBtn;

    [SerializeField] private GameObject gemShopIcon;
    [SerializeField] private GameObject premiumShopIcon;
    [SerializeField] private GameObject gemShopInfoScreen;
    [SerializeField] private GameObject premiumShopInfoScreen;

    [SerializeField] private GemShop gemShop;
    private bool infoScreenIsOpen = false;

    private void Start()
    {
        Init();
    }

    public override MenuName Menu => MenuName.MainShop;


    public override void Init()
    {
        gemShopBtn.onClick.AddListener(OnClick_GemShopButton);
        gemBueBtn.onClick.AddListener(OnClick_GemShopButton);
        premiumShopBtn.onClick.AddListener(OnClick_PremiumShopButton);
        premiumBueBtn.onClick.AddListener(OnClick_PremiumShopButton);
        gemShopInfoBtn.onClick.AddListener (OnClick_GemShopInfoButton);
        premiumShopInfoBtn.onClick.AddListener(OnClick_PremiumShopInfoButton);
        backBtn.onClick.AddListener(OnClick_BackButton);
    }

    private void OnClick_GemShopButton()
    {
        //add invoke method of gem shop to set previous screen 
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        InfoScreenController(false);
        MainMenuManager.Instance.ChangeMenu(MenuName.GemShop);
        
    }
    private void OnClick_PremiumShopButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        InfoScreenController(false);
        MainMenuManager.Instance.ChangeMenu(MenuName.PremiumShop);
    }

    private void OnClick_GemShopInfoButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if(infoScreenIsOpen) 
        {
            InfoScreenController(false);
        }
        else
        {
            InfoScreenController(true, gemShopInfoScreen);
        }
        
    }
    private void OnClick_PremiumShopInfoButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);        
        if (infoScreenIsOpen)
        {
            InfoScreenController(false);
        }
        else
        {
            InfoScreenController(true, premiumShopInfoScreen);
        }
    }
    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (infoScreenIsOpen)
        {
            InfoScreenController(false);
        }
        else
        {
            MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);
        }
    }

    private void InfoScreenController(bool shouldOpen, GameObject infoScreen = null) 
    {
        if (shouldOpen && infoScreen != null)
        {
            infoScreenIsOpen = true;
            if (infoScreen == gemShopInfoScreen)
            {
                gemShopIcon.SetActive(true);
                premiumShopIcon.SetActive(false);
                gemShopInfoScreen.SetActive(true);
                premiumShopInfoScreen.SetActive(false);

            }
            if(infoScreen == premiumShopInfoScreen)
            {
                gemShopIcon.SetActive(false);
                premiumShopIcon.SetActive(true);
                gemShopInfoScreen.SetActive(false);
                premiumShopInfoScreen.SetActive(true);
            }

        }
        else
        {
            gemShopIcon.SetActive(true);
            premiumShopIcon.SetActive(true);
            gemShopInfoScreen.SetActive(false);
            premiumShopInfoScreen.SetActive(false);
            infoScreenIsOpen = false;

        }
    }
    


}
