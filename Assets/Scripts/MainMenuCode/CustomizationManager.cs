using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] private GameObject avatar;
    [SerializeField] private GameObject avatarBack;
    [SerializeField] private GameObject nameBack;
    [SerializeField] private TMP_Text playerName;

    [SerializeField] private ImagesPack avatarsImages;
    [SerializeField] private ImagesPack avatarsBackImages;
    [SerializeField] private ImagesPack nameBackImages;

    [SerializeField] private GameObject avatarsMenu;
    [SerializeField] private GameObject nameBackMenu;
    [SerializeField] private GameObject avatarBacksMenu;

    [SerializeField] private GameObject customizationButtonPrefab;

    private PlayerScript playerToChange;
    private int playerAvatarIndex;
    private int playerAvatarBackIndex;
    private int playerNameBackIndex;


    private void Awake()
    {
        GenerateCustomizationMenuElements();
    }



    public void SetCustomizationPreview(PlayerScript player)
    {
        playerToChange = player;
        playerAvatarIndex = player.avatarID;
        playerAvatarBackIndex = player.avatarBackID;
        playerAvatarBackIndex = player.nameBackID;
        playerName.text = player.name;
        UpdatePreviewImage();

    }

    private void UpdatePreviewImage()
    {
        if (playerToChange != null)
        {
            avatar.GetComponent<Image>().sprite = avatarsImages.someImage[playerAvatarIndex];
            avatarBack.GetComponent<Image>().sprite = avatarsBackImages.someImage[playerAvatarBackIndex];
            nameBack.GetComponent<Image>().sprite = nameBackImages.someImage[playerNameBackIndex];

        }
    }

    private void GenerateCustomizationMenuElements()
    {
        foreach (var itemImage in avatarsImages.someImage)
        {
            GameObject newElement = Instantiate(customizationButtonPrefab, avatarsMenu.transform);
            CustomizationButton cus = newElement.GetComponentInChildren<CustomizationButton>();
            cus.SetInfield(itemImage);
            cus.index = avatarsImages.someImage.IndexOf(itemImage);
            cus.part = CustomizationPart.Avatar;
            newElement.GetComponentInChildren<Button>().onClick.AddListener(() => SetCustomizationElement(cus)); ;


        }

    }

    private void SetCustomizationElement(CustomizationButton element)
    {
        switch (element.part)
        {
            case CustomizationPart.Avatar:
                playerAvatarIndex = element.index;
                break;
            case CustomizationPart.AvatarBack:
                playerAvatarBackIndex = element.index;
                break;
            case CustomizationPart.NameBack:
                playerNameBackIndex = element.index;
                break;
            default:
                Debug.LogWarning("�������� ������ �����������!");
                break;
        }
        UpdatePreviewImage();
    }




}

public enum CustomizationPart
{
    Avatar,
    AvatarBack,
    NameBack,
}
