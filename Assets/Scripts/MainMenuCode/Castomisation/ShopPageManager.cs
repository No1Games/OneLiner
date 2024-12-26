using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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



}


