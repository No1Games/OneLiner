using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomizationDataManager : MonoBehaviour
{
    [SerializeField] private AccountManager accountManager;
    private PlayerScript currentPlayer;
    private string newPlayerName = null;
    private MenuName previousMenuName;
    
    private List<Item> itemsOnPreview = new();

    public event Action OnCustomizationEnds;

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
        int _newAvatarId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.Avatars)?.itemCode ?? currentPlayer.avatarID;
        int _newAvatarBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.AvatarBackgrounds)?.itemCode ?? currentPlayer.avatarBackID;
        int _newNameBackId = itemsOnPreview.FirstOrDefault(i => i.category == ItemCategory.NameBackgrounds)?.itemCode ?? currentPlayer.nameBackID;

        if(newPlayerName == null)
        {
            Debug.Log("name is empty");
            newPlayerName = currentPlayer.name;

        }
        
        currentPlayer.UpdatePlayerInfo(newPlayerName, _newAvatarId, _newAvatarBackId, _newNameBackId);

        if(currentPlayer == accountManager.player)
        {
            accountManager.GetAccountData().avatarCode = _newAvatarId;
            accountManager.GetAccountData().avatarBackgroundCode = _newAvatarBackId;
            accountManager.GetAccountData().nameBackgroundCode = _newNameBackId;
            accountManager.SaveAccount();
        }
        
        OnCustomizationEnds?.Invoke();
        ClearData();
    }


}
