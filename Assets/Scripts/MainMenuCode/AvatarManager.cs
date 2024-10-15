using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour
{
    [SerializeField] private ImagesPack avatars;
    [SerializeField] private ImagesPack avatarsBack;
    [SerializeField] private ImagesPack nameBack;
    [SerializeField] private AccountManager accountManager;
    [SerializeField] private GameObject avatarMainMenu;
    [SerializeField] private GameObject avatarBackMainMenu;
    // Start is called before the first frame update
    void Start()
    {
        SetStartScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SetStartScreen()
    {
        if(accountManager.player != null)
        {
            avatarMainMenu.GetComponent<Image>().sprite = avatars.someImage[accountManager.player.avatarID];
            avatarBackMainMenu.GetComponent<Image>().sprite = avatarsBack.someImage[accountManager.player.avatarBackID];
        }
    }
}
