using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CustomizationMenuUi : MenuBase
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button applyBtn;
    [SerializeField] private Button avatarShopBtn;
    [SerializeField] private Button avatarBackShopBtn;
    [SerializeField] private Button nameBackShopBtn;
    
    [SerializeField] private List<Tab> tabs;

    public override MenuName Menu => MenuName.CustomizationScreen;

       
    public event Action OnApplyBtnClick;



    private void Awake()
    {
        Init();
        
    }

    public override void Init()
    {
        base.Init();
        SetActiveTab(1);
        avatarShopBtn.onClick.AddListener(OnClick_avatarShopBtn);
        avatarBackShopBtn.onClick.AddListener(OnClick_avatarBackShopBtn);
        nameBackShopBtn.onClick.AddListener(OnClick_nameBackShopBtn);
        backBtn.onClick.AddListener(OnClick_backButton);
        applyBtn.onClick.AddListener(OnClick_applyButton);

    }

    private void OnClick_backButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);
    }
    private void OnClick_applyButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_avatarShopBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        SetActiveTab(1);
    }
    private void OnClick_avatarBackShopBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        SetActiveTab(2);
    }
    private void OnClick_nameBackShopBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        SetActiveTab(3);
    }


    public void SetActiveTab(int tabIndex)
    {
        
        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];

            if (i == tabIndex-1)
            {
                tab.activeImage.SetActive(true);      
                tab.inactiveImage.SetActive(false);   
                tab.contentPanel.SetActive(true);     
            }
            else
            {
                tab.activeImage.SetActive(false);     
                tab.inactiveImage.SetActive(true);    
                tab.contentPanel.SetActive(false);    
            }
        }
    }


}
