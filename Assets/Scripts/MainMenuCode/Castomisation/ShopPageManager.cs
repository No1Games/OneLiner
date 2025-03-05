
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPageManager : MonoBehaviour
{
    [SerializeField] private List<Image> shownPlaces;
        
    [Header("Prefabs & References")]
    [SerializeField] private GameObject buttonPrefab; // Префаб кнопки
    [SerializeField] private Transform buttonParent;  // Батьківський об'єкт для кнопок
    [SerializeField] private AccountManager accountManager;
    [SerializeField] private CustomizationMenuUi uiMenu;

    [Header("Item Category")]
    [SerializeField] private ItemCategory category; // Категорія айтемів для цієї сторінки

    private List<GameObject> buttons = new List<GameObject>(); // Список створених кнопок


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
        // Отримуємо всі коди айтемів цієї категорії
        List<int> itemCodes = ItemManager.Instance.GetItemCodesByCategory(category);

        foreach (int code in itemCodes)
        {
            Item item = ItemManager.Instance.GetItemByCode(code);

            // Створюємо кнопку
            GameObject button = Instantiate(buttonPrefab, buttonParent);

            // Кастомізація кнопки
            ItemButton btnFunc = button.GetComponent<ItemButton>();
            btnFunc.CustomizeButton(item, CheckItemAccessibility(item));

            // Підписка кнопки на методи
            btnFunc.AssignMainButtonAction(SetItem);
            btnFunc.AssignPriceButtonAction(BueItem);


            // Додаємо кнопку в список
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

            // 1. Спочатку доступні айтеми
            if (availableA != availableB)
            {
                return availableA ? -1 : 1;
            }

            // 2. Потім сортуємо заблоковані за рівнем
            if (itemA.requiredLevel != itemB.requiredLevel)
            {
                return itemA.requiredLevel.CompareTo(itemB.requiredLevel);
            }

            // 3. Потім сортуємо за ціною
            return itemA.cost.CompareTo(itemB.cost);
        });

        // Оновлюємо ієрархію об'єктів у buttonParent
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].transform.SetSiblingIndex(i);
        }
    }



}


