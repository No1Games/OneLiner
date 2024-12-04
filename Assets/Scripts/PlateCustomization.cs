using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlateCustomization : MonoBehaviour
{

    [SerializeField] TMP_Text playerName;
    [SerializeField] GameObject back;
    [SerializeField] GameObject avatar;
    [SerializeField] GameObject avatarBack;
    public GameObject additionalMenu;
    public GameObject leaderCrown;

    [SerializeField] private Button mainBtn;
    [SerializeField] private Button firstMiniBtn;
    [SerializeField] private Button secondMiniBtn;
    
    
    public Button FirstMiniButton => firstMiniBtn;
    
    public Button SecondMiniButton => secondMiniBtn;
    public Button MainButton => mainBtn;

    public PlayerScript player;
      

    public void SetName(string name)
    {
        playerName.text = name;
    }

    public void SetBack(Sprite back)
    {
        this.back.GetComponent<Image>().sprite = back;
    }
    public void SetAvatarBack(Sprite avatarBack)
    {
        this.avatarBack.GetComponent<Image>().sprite = avatarBack;
    }

    public void SetAvatar(Sprite avatar)
    {
        this.avatar.GetComponent<Image>().sprite = avatar;
    }

    public void SetPlayer(PlayerScript player)
    {
        this.player = player;
    }

    public void CustomizePlate(PlayerScript player)
    {
        

        SetName(player.name);
        SetAvatar(ItemManager.Instance.GetItemByCode(player.avatarID).icon);
        SetBack(ItemManager.Instance.GetItemByCode(player.nameBackID).icon);
        SetAvatarBack(ItemManager.Instance.GetItemByCode(player.avatarBackID).icon);
    }   
    
    
}
