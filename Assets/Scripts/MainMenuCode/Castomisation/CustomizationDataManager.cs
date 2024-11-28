using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomizationDataManager : MonoBehaviour
{
    [SerializeField] private AccountManager accountManager;
    private PlayerScript currentPlayer;
    private MenuName previousMenuName;
    
    private List<Item> itemsOnPreview = new();

    public event Action OnCustomizationEnds;

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

    public void ReturnDataToPreviousMenu()
    {
        int _newAvatarId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.Avatars)?.itemCode ?? currentPlayer.avatarID;
        int _newAvatarBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.AvatarBackgrounds)?.itemCode ?? currentPlayer.avatarBackID;
        int _newNameBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.NameBackgrounds)?.itemCode ?? currentPlayer.nameBackID;

        
        currentPlayer.UpdatePlayerInfo("Samurai", _newAvatarId, _newAvatarBackId, _newNameBackId);

        if(currentPlayer == accountManager.player)
        {
            accountManager.GetAccountData().avatarCode = _newAvatarId;
            accountManager.GetAccountData().avatarBackgroundCode = _newAvatarBackId;
            accountManager.GetAccountData().nameBackgroundCode = _newNameBackId;
            accountManager.SaveAccount();
        }
        
        OnCustomizationEnds?.Invoke();
    }


}
