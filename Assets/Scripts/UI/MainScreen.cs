using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreen : MenuBase
{
    [SerializeField] private Button shopBtn;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button optionsBtn;
    [SerializeField] private Button customizationBtn;

    public override MenuName Menu => MenuName.MainScreen;

    private void Awake()
    {
        Init();
    }
    public override void Init()
    {
        playBtn.onClick.AddListener(OnClick_PlayBtn);
        shopBtn.onClick.AddListener(OnClick_ShopBtn);
        optionsBtn.onClick.AddListener(OnClick_OptionsBtn);
        customizationBtn.onClick.AddListener(OnClick_CustomizationBtn);

    }

    private void OnClick_PlayBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);
        
    }
    private void OnClick_OptionsBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_ShopBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);

    }
    private void OnClick_CustomizationBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }

}

