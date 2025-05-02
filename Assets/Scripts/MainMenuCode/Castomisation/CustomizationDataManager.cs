using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomizationDataManager : MonoBehaviour
{
    
    [SerializeField] private PopUpController popupController;
    private PlayerScript currentPlayer;
    private string newPlayerName = null;
    private MenuName previousMenuName;
    
    private List<Item> itemsOnPreview = new();
    [SerializeField] private List<ShopPageManager> shopPageManagers = new();
    [SerializeField] private CustomizationMenuUi ui;

    public event Action OnCustomizationEnds;

    private void Start()
    {
        popupController.OnPurchaseConfirmed += PurchaseItem;
        popupController.OnExitConfirmed += (() =>
        {
            MainMenuManager.Instance.ChangeMenu(previousMenuName);
            ClearData();
        });
    }
    
    public void SetNewPlayerName(string name)
    {
        newPlayerName = name;
    }
    public PlayerScript GetPlayer()
    {
        return currentPlayer;
    }
    
    public MenuName GetPreviousMenuName()
    {
        return previousMenuName;
    }
    public void SetupData(PlayerScript player, MenuName menu )
    {
        currentPlayer = player;
        
        previousMenuName = menu;
    }
    public void AddItems(Item item)
    {        
        itemsOnPreview.RemoveAll(i => i.category == item.category);
        itemsOnPreview.Add(item);

    }
    public List<Item> GetItems()
    {
        return new List<Item>(itemsOnPreview);
    }
    private void ClearData()
    {
        itemsOnPreview.Clear();
        newPlayerName = null;
        currentPlayer= null;

    }
    public void ReturnDataToPreviousMenu()
    {
        bool canUpdate = true;
        foreach (Item item in itemsOnPreview)
        {
            if (!AccountManager.Instance.CurrentAccountData.cosmeticCodes.Contains(item.itemCode))
            {
                canUpdate = false;
            }
        }
        if (canUpdate)
        {
            UpdatePlayerCustomizationData();
        }
        

        OnCustomizationEnds?.Invoke();
        ClearData();
    }

    private void UpdatePlayerCustomizationData()
    {
        int _newAvatarId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.Avatars)?.itemCode ?? currentPlayer.avatarID;
        int _newAvatarBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.AvatarBackgrounds)?.itemCode ?? currentPlayer.avatarBackID;
        int _newNameBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.NameBackgrounds)?.itemCode ?? currentPlayer.nameBackID;

        if (newPlayerName == null)
        {
            Debug.Log("name is empty");
            newPlayerName = currentPlayer.name;

        }

        currentPlayer.UpdatePlayerInfo(newPlayerName, _newAvatarId, _newAvatarBackId, _newNameBackId);

        if (currentPlayer == AccountManager.Instance.player)
        {
            AccountManager.Instance.CurrentAccountData.avatarCode = _newAvatarId;
            AccountManager.Instance.CurrentAccountData.avatarBackgroundCode = _newAvatarBackId;
            AccountManager.Instance.CurrentAccountData.nameBackgroundCode = _newNameBackId;
            AccountManager.Instance.CurrentAccountData.playerName = newPlayerName;
            AccountManager.Instance.SaveAccountData();
        }
    }

    

    public void ApplyCustomization()
    {
        List<Item> _notOwnedItems = new List<Item>();
        int _price = 0;
        bool itemsIsLockedByPrice = false;
        bool itemsIsLockedByLvl = false;
        foreach (Item item in itemsOnPreview)
        {
            if (!AccountManager.Instance.CurrentAccountData.cosmeticCodes.Contains(item.itemCode))
            {
                _price += item.cost;
                _notOwnedItems.Add(item);

                if(item.cost > 0)
                {
                    itemsIsLockedByPrice = true;
                }
                if(item.requiredLevel > 0)
                {
                    itemsIsLockedByLvl = true;
                }
            }
        }
        if(_notOwnedItems.Count > 0)
        {
            if(itemsIsLockedByPrice && !itemsIsLockedByLvl)
            {
                if (_price <= GemManager.Instance.GetGems())
                {
                    popupController.OpenPopUp_ApproveWithGems(_notOwnedItems);
                }
                else
                {
                    popupController.OpenPopUp_ApproveWithNoGems(_price - GemManager.Instance.GetGems());
                }
                
            }
            else if (itemsIsLockedByLvl)
            {
                if(AccountManager.Instance.CurrentAccountData.accountStatus == AccountStatus.Premium)
                {
                    popupController.OpenPopUp_ApproveWithPremiumNoLvl();
                }
                else
                {
                    popupController.OpenPopUp_ApproveWithNoPremiumNoLvl();
                }
            }
            else
            {
                popupController.OpenPopUp_ApproveCombine();
            }

        }
        else
        {
            UpdatePlayerCustomizationData();
        }
    }

    public void PurchaseItem(List<Item> items)
    {
        int _price = 0;
        foreach(Item item in items)
        {
            _price += item.cost;

        }
        GemManager.Instance.SpendGems(_price);
        AccountManager.Instance.AddItemsToAccount(items);

       // update customisation menu prewiev decrise gem ammount sort and filter items in shop
       foreach(ShopPageManager shop in shopPageManagers)
        {
            if (shop.gameObject.activeSelf)
            {
                shop.UpdateItemVisibility();

            }
        }
       ui.UpdateGemsAtShop();
    }

    public void SigleBuy(Item item)
    {
        if (GemManager.Instance.GetGems() >= item.cost)
        {
            popupController.OpenPopUp_PurchaseSkinWithGems(item);
        }
        else
        {
            popupController.OpenPopUp_PurchaseSkinWithNoGems();
        }
    }

    public bool CheckExitAbility()
    {
        foreach (Item item in itemsOnPreview)
        {
            if (item.itemCode != currentPlayer.avatarID &&
                item.itemCode != currentPlayer.avatarBackID &&
                item.itemCode != currentPlayer.nameBackID)
            {
                popupController.OpenPopUp_LeaveWithoutChanges();

               
                return false;
            }
        }
        return true;
    }
}
