
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class CustomizationMenuUi : MenuBase
{
    [Header("Navigation")]
    [SerializeField] private Button backBtn;
    [SerializeField] private Button applyBtn;
    [SerializeField] private Button avatarShopBtn;
    [SerializeField] private Button avatarBackShopBtn;
    [SerializeField] private Button nameBackShopBtn;
    
    [SerializeField] private List<Tab> tabs;

    

    [Header("Preview")]
    [SerializeField] CustomizationDataManager customizationDataManager;
    [SerializeField] private Image avatarBig;
    [SerializeField] private Image avatarSmall;
    [SerializeField] private Image avatarBackBig;
    [SerializeField] private Image avatarBackSmall;
    [SerializeField] private Image nameBackImg;
    [SerializeField] private TMP_InputField playerNameInputField;

    [SerializeField] private TextMeshProUGUI gems;





    public override MenuName Menu => MenuName.CustomizationScreen;
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

        

        playerNameInputField.onEndEdit.AddListener(OnPlayerNameEditComplete);
        



    }
    public override void Show()
    {
        base.Show();
        UpdatePreview(customizationDataManager.GetPlayer());
    }
    private void OnClick_backButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);

        
        if (customizationDataManager.CheckExitAbility())
        {

            customizationDataManager.ReturnDataToPreviousMenu();
            MainMenuManager.Instance.ChangeMenu(customizationDataManager.GetPreviousMenuName());

        }

        
    }
   
    private void OnClick_applyButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        customizationDataManager.ApplyCustomization();
        


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

    // оновлюЇ вигл€д вс≥х компонент≥в на основ≥ даних гравц€.
    void UpdatePreview(PlayerScript player) 
    {
        avatarBig.sprite = ItemManager.Instance.GetItemByCode(player.avatarID).icon;
        avatarSmall.sprite = ItemManager.Instance.GetItemByCode(player.avatarID).icon;
        avatarBackBig.sprite = ItemManager.Instance.GetItemByCode(player.avatarBackID).icon;
        avatarBackSmall.sprite = ItemManager.Instance.GetItemByCode(player.avatarBackID).icon;
        nameBackImg.sprite = ItemManager.Instance.GetItemByCode(player.nameBackID).icon;
        playerNameInputField.text = player.name;
        UpdateGemsAtShop();
    }

    public void UpdateGemsAtShop()
    {
        gems.text = GemManager.Instance.GetGems().ToString();
    }

    //оновлюЇ т≥льки певну частину превТю (наприклад, аватар чи фон).
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
                //  од, €кщо жоден case не виконавс€
                Debug.Log("Item not found");
                break;
            }
        customizationDataManager.AddItems(item);
        }
    private void OnPlayerNameEditComplete(string inputName)
    {
        // ѕерев≥р€Їмо, чи введенн€ Ї вал≥дним
        if (string.IsNullOrWhiteSpace(inputName))
        {
            // якщо поле пусте, повертаЇмо попереднЇ значенн€
            playerNameInputField.text = customizationDataManager.GetPlayer().name;
            Debug.LogWarning("Name cannot be empty! Reverting to previous name.");
        }
        else if (inputName.Length > 13)
        {
            // якщо текст довший за 13 символ≥в, обр≥заЇмо його
            string truncatedName = inputName.Substring(0, 13);
            playerNameInputField.text = truncatedName;
            customizationDataManager.SetNewPlayerName(playerNameInputField.text);
            Debug.LogWarning("Name too long! Truncated to 13 characters.");
        }
        else
        {
            // якщо текст вал≥дний, збер≥гаЇмо його
            customizationDataManager.SetNewPlayerName(playerNameInputField.text);
            Debug.Log($"Name successfully updated to: {inputName}");
        }
    }

    public void StartSinglePurchase(Item item)
    {
        customizationDataManager.SigleBuy(item);
    }


}
