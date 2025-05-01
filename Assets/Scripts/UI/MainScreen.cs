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
    [SerializeField] private CustomizationDataManager customizationDataManager;
    [SerializeField] private TMPro.TextMeshProUGUI gems;
    

    [SerializeField] private Image avatarMainMenu;
    [SerializeField] private Image avatarBackMainMenu;

    

    public override MenuName Menu => MenuName.MainScreen;

    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        AccountManager.Instance.OnAccountInitializationComplete += SetupMainScreenView;
        customizationDataManager.OnCustomizationEnds += SetupMainScreenView;
    }
    public override void Init()
    {
        playBtn.onClick.AddListener(OnClick_PlayBtn);
        shopBtn.onClick.AddListener(OnClick_ShopBtn);
        optionsBtn.onClick.AddListener(OnClick_OptionsBtn);
        customizationBtn.onClick.AddListener(OnClick_CustomizationBtn);  

    }
    public override void Show()
    {
        base.Show();
        SetupMainScreenView();
    }

    private void OnClick_PlayBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);
        
    }
    private void OnClick_OptionsBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.OpenMenu(MenuName.OptionScreen);
    }
    private void OnClick_ShopBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainShop);

    }
    private void OnClick_CustomizationBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        customizationDataManager.SetupData(AccountManager.Instance.player, MenuName.MainScreen);
        MainMenuManager.Instance.ChangeMenu(MenuName.CustomizationScreen);
    }

    private void SetupMainScreenView()
    {
        avatarMainMenu.sprite = ItemManager.Instance.GetItemByCode(AccountManager.Instance.player.avatarID).icon;
        avatarBackMainMenu.sprite = ItemManager.Instance.GetItemByCode(AccountManager.Instance.player.avatarBackID).icon;
        gems.text = GemManager.Instance.GetGems().ToString();
    }

}

