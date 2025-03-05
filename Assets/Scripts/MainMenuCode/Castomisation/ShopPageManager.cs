
using System.Collections.Generic;
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
        UpdateItemVisibility();

    }
    private void OnEnable()
    {
        UpdateItemVisibility();
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

    public void UpdateItemVisibility()
    {
        foreach(GameObject button in buttons)
        {
            ItemButton _btnFunc = button.GetComponent<ItemButton>();
            bool _isAvaliable = CheckItemAccessibility(_btnFunc.GetItemData());
            if (_isAvaliable)
            {
                _btnFunc.HideRequirements();
                
            }

        }
        SortButtons();
    }
    private void SortButtons()
    {
        buttons.Sort((a, b) =>
        {
            ItemButton buttonA = a.GetComponent<ItemButton>();
            ItemButton buttonB = b.GetComponent<ItemButton>();

            Item itemA = buttonA.GetItemData();
            Item itemB = buttonB.GetItemData();

            bool availableA = CheckItemAccessibility(itemA);
            bool availableB = CheckItemAccessibility(itemB);

            // 1. �������� ������� ������
            if (availableA != availableB)
            {
                return availableA ? -1 : 1;
            }

            // 2. ���� ������� ���������� �� �����
            if (itemA.requiredLevel != itemB.requiredLevel)
            {
                return itemA.requiredLevel.CompareTo(itemB.requiredLevel);
            }

            // 3. ���� ������� �� �����
            return itemA.cost.CompareTo(itemB.cost);
        });

        // ��������� �������� ��'���� � buttonParent
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.SetSiblingIndex(i);
        }
    }



}


