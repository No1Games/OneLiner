using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopPageManager : MonoBehaviour
{
    [SerializeField] private List<Image> shownPlaces;
        
    [Header("Prefabs & References")]
    [SerializeField] private GameObject buttonPrefab; // ������ ������
    [SerializeField] private Transform buttonParent;  // ����������� ��'��� ��� ������
    [SerializeField] private AccountManager accountManager;
    [SerializeField] private CustomizationMenuUi uiMenu;

    [Header("Item Category")]
    [SerializeField] private ItemCategory category; // �������� ������ ��� ���� �������

    private List<GameObject> buttons = new List<GameObject>(); // ������ ��������� ������


    private void Start()
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        // �������� �� ���� ������ ���� �������
        List<int> itemCodes = ItemManager.Instance.GetItemCodesByCategory(category);

        foreach (int code in itemCodes)
        {
            Item item = ItemManager.Instance.GetItemByCode(code);

            // ��������� ������
            GameObject button = Instantiate(buttonPrefab, buttonParent);

            // ����������� ������
            ItemButton btnFunc = button.GetComponent<ItemButton>();
            btnFunc.CustomizeButton(item, CheckItemAccessibility(item));

            // ϳ������ ������ �� ������
            btnFunc.AssignMainButtonAction(SetItem);
            btnFunc.AssignPriceButtonAction(BueItem);


            // ������ ������ � ������
            buttons.Add(button);
        }
    }

    private bool CheckItemAccessibility(Item item)
    {
        
        foreach (int accountIemCode in accountManager.GetAccountData().cosmeticCodes)
        {
            if(accountIemCode == item.itemCode)
            {
                return true;
            }
        }
        return false;
    }

    private void SetItem(Item item)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        uiMenu.UpdateSpecific(item);

    }
    private void BueItem(Item item)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        uiMenu.StartSinglePurchase(item);
    }



}


