using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlateCustomization : MonoBehaviour
{

    [SerializeField] TMP_Text playerName;
    [SerializeField] GameObject back;
    [SerializeField] GameObject avatar;
    [SerializeField] GameObject avatarBack;

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

    public void CustomizePlate(AvatarManager avatarManager, PlayerScript player)
    {
        

        SetName(player.name);
        SetAvatar(avatarManager.GetAvatarImage(player.avatarID));
        SetBack(avatarManager.GetNameBackImage(player.nameBackID));
        SetAvatarBack(avatarManager.GetAvatarBackImage(player.avatarBackID));
    }
}
