using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CustomizationMenuUi : MenuBase
{
    [Header("Navigation")]
    [SerializeField] private Button backBtn;
    [SerializeField] private Button applyBtn;
    [SerializeField] private Button avatarShopBtn;
    [SerializeField] private Button avatarBackShopBtn;
    [SerializeField] private Button nameBackShopBtn;
    
    [SerializeField] private List<Tab> tabs;

    [Header("Purchase PopUp")]
    [SerializeField] private GameObject smallPurchasePanel;
    [SerializeField] private Button sppYesBtn;
    [SerializeField] private Button sppNoBtn;

    [SerializeField] private GameObject notEnoughGemsPanel;
    [SerializeField] private Button negNoBtn;
    [SerializeField] private Button negYesBtn;

    [Header("Preview")]
    [SerializeField] CustomizationDataManager customizationDataManager;
    [SerializeField] private Image avatarBig;
    [SerializeField] private Image avatarSmall;
    [SerializeField] private Image avatarBackBig;
    [SerializeField] private Image avatarBackSmall;
    [SerializeField] private Image nameBackImg;


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

        negNoBtn.onClick.AddListener(OnClick_closePopUp);
        negYesBtn.onClick.AddListener(OnClick_applyButton);
        sppYesBtn.onClick.AddListener(OnClick_sppYesButton);
        sppNoBtn.onClick.AddListener(OnClick_closePopUp);       

    }
    public override void Show()
    {
        base.Show();
        UpdatePreview(customizationDataManager.GetPlayer());
    }

    private void OnClick_backButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);
    }
    private void OnClick_applyButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        customizationDataManager.ReturnDataToPreviousMenu();
        MainMenuManager.Instance.ChangeMenu(customizationDataManager.GetPreviousMenuName());
        
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

    public void ShowPopup(Item item)
    {
        // Логіка відображення попапу
        
        if (GemManager.Instance.GetGems() >= item.cost)
        {
            smallPurchasePanel.SetActive(true);
        }
        else
        {
            notEnoughGemsPanel.SetActive(true);
        }

    }

    private void OnClick_sppYesButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        //remove gems
        //update shop icons
    }
    private void OnClick_negYesButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        //open gem shop screen
        
    }
    private void OnClick_closePopUp()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        smallPurchasePanel.SetActive(false);
        notEnoughGemsPanel.SetActive(false);

    }

    // оновлює вигляд всіх компонентів на основі даних гравця.
    void UpdatePreview(PlayerScript player) 
    {
        avatarBig.sprite = ItemManager.Instance.GetItemByCode(player.avatarID).icon;
        avatarSmall.sprite = ItemManager.Instance.GetItemByCode(player.avatarID).icon;
        avatarBackBig.sprite = ItemManager.Instance.GetItemByCode(player.avatarBackID).icon;
        avatarBackSmall.sprite = ItemManager.Instance.GetItemByCode(player.avatarBackID).icon;
        nameBackImg.sprite = ItemManager.Instance.GetItemByCode(player.nameBackID).icon;

    }

    //оновлює тільки певну частину прев’ю (наприклад, аватар чи фон).
    public void UpdateSpecific(Item item) 
    {
        switch(item.category)
{
            case ItemCategory.Avatars:
                avatarBig.sprite = item.icon;
                avatarSmall.sprite = item.icon;
                break;
            case ItemCategory.AvatarBackgrounds:
                avatarBackBig.sprite = item.icon;
                avatarBackSmall.sprite = item.icon;
                break;
            case ItemCategory.NameBackgrounds:
                nameBackImg.sprite = item.icon;
                break;
            default:
                // Код, якщо жоден case не виконався
                Debug.Log("Item not found");
                break;
            }
        customizationDataManager.AddItems(item);
        }
}
