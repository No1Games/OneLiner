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

    public override void Init()
    {
        playBtn.onClick.AddListener(OnClick_PlayBtn);

    }

    private void OnClick_PlayBtn()
    {
        MainMenuManager.Instance.ChangeMenu(MenuName.LocalOnline);
    }
    private void OnClick_OptionsBtn()
    {

    }
    private void OnClick_ShopBtn()
    {

    }
    private void OnClick_CustomizationBtn()
    {

    }

}

