using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GameSetupMenu : MonoBehaviour
{
    
    [SerializeField] private GameObject playerPlatePrefab;
    [SerializeField] private GameObject addPlayerBtn;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private AvatarManager avatarManager;
    [SerializeField] private AccountManager accountManager;

   
    private List<GameObject> playersPlates = new();


    [SerializeField] private GameObject avatarMainMenu;
    [SerializeField] private GameObject avatarBackMainMenu;

    [SerializeField] private GameObject customizationMenu;
    [SerializeField] private CustomizationManager customizationManager;

    


    private void Awake()
    {
        
        customizationManager.onChangesAccepted += CloseCustomization;
        //customizationManager.onChangesAccepted += UpdatePlayerData;
    }
    private void Start()
    {
        SetStartScreen();
    }
    void SetStartScreen()
    {
        if (accountManager.player != null)
        {
            avatarMainMenu.GetComponent<Image>().sprite = avatarManager.GetAvatarImage(accountManager.player.avatarID);
            avatarBackMainMenu.GetComponent<Image>().sprite = avatarManager.GetAvatarBackImage(accountManager.player.avatarBackID);
        }
    }
    public void OpenMainCustomization()
    {
        customizationMenu.SetActive(true);
        customizationManager.SetCustomizationPreview(accountManager.player);

    }
    public void OpenCustomizationFromPlate(PlayerScript player)
    {
        customizationMenu.SetActive(true);
        customizationManager.SetCustomizationPreview(player);
    }
    public void CloseCustomization(PlayerScript updatedPlayer)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        customizationMenu.SetActive(false);
        if (playersPlates.Count > 0)
        {
            foreach(GameObject p in playersPlates)
            {
                if (p.GetComponent<PlateCustomization>().player == updatedPlayer)
                {
                    p.GetComponent<PlateCustomization>().CustomizePlate(avatarManager, updatedPlayer);
                }
            }
        }
        SetStartScreen();

    }
    //private void UpdatePlayerData(PlayerScript playerToSave)
    //{
    //    if (playerToSave == accountManager.player)
    //    {
    //        accountManager.SavePlayerData();
    //    }
    //}


    

}
