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

    [SerializeField] private GameObject tutorial;
    [SerializeField] private TutorialReward tutorialReward;
    [SerializeField] private GameObject specialOfferButton;
    [SerializeField] private GameObject specialOfferScreen;
    
    

    public override MenuName Menu => MenuName.MainScreen;

    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        AccountManager.Instance.OnAccountInitializationComplete += SetupMainScreenView;
        AccountManager.Instance.OnAccountInitializationComplete += FirstShowOfSpecialOffer;
        customizationDataManager.OnCustomizationEnds += SetupMainScreenView;
        tutorialReward.OnRewardEarned += UpdateGems;
    }
    private void OnDisable()
    {
        AccountManager.Instance.OnAccountInitializationComplete -= SetupMainScreenView;
        AccountManager.Instance.OnAccountInitializationComplete -= FirstShowOfSpecialOffer;
        customizationDataManager.OnCustomizationEnds -= SetupMainScreenView;
    }
    public override void Init()
    {
        playBtn.onClick.AddListener(OnClick_PlayBtn);
        shopBtn.onClick.AddListener(OnClick_ShopBtn);
        optionsBtn.onClick.AddListener(OnClick_OptionsBtn);
        customizationBtn.onClick.AddListener(OnClick_CustomizationBtn);

        specialOfferButton.GetComponent<Button>().onClick.AddListener(OnSpecialOfferClick);



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
        UpdateGems();

        Debug.Log("Account tutor status is " + AccountManager.Instance.CurrentAccountData.tutorialStatus);
        if (AccountManager.Instance.CurrentAccountData.tutorialStatus == TutorialStatus.None && !IngameData.Instance.IsTutorialOn)
        {
            tutorial.SetActive(true);
        }

        if (!AccountManager.Instance.CurrentAccountData.specialOfferEarned) 
        {
            specialOfferButton.SetActive(true);
        }
        if (IngameData.Instance.IsTutorialOn && AccountManager.Instance.CurrentAccountData.tutorialStatus != TutorialStatus.Completed)
        {
            tutorialReward.gameObject.SetActive(true);
        }

    }
    void UpdateGems()
    {
        gems.text = GemManager.Instance.GetGems().ToString();
    }

    private void OnSpecialOfferClick()
    {
        
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
            specialOfferScreen.SetActive(true);
        
    }
    private void FirstShowOfSpecialOffer()
    {
        AccountData data = AccountManager.Instance.CurrentAccountData;
        if (!data.specialOfferEarned && data.tutorialStatus != TutorialStatus.None )
        {
            specialOfferScreen.SetActive(true);
        }

    }

}

