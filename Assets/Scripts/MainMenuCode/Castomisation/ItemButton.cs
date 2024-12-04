using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button mainButton; // Головна кнопка
    [SerializeField] private Button priceButton; // Кнопка покупки
    [SerializeField] private Image itemIcon; // Іконка айтема
    [SerializeField] private Image lockIcon; // Замочок
    [SerializeField] private GameObject levelIcon; // Значок рівня
    [SerializeField] private GameObject priceIcon; // Значок ціни
    [SerializeField] private TextMeshProUGUI priceText; // Ціна
    [SerializeField] private TextMeshProUGUI levelRequirementText; // Вимога до рівня

    [Header("State")]
    private Item itemData; // Дані айтема, які налаштовують кнопку

    public void CustomizeButton(Item data, bool isAvailable)
    {
        itemData = data;
        itemIcon.sprite = data.icon;

        if (isAvailable)
        {
            lockIcon.gameObject.SetActive(false);
            priceIcon.SetActive(false);
            levelIcon.SetActive(false);
            
            
        }
        else
        {
            lockIcon.gameObject.SetActive(true);

            if (data.requiredLevel > 0)
            {
                levelIcon.SetActive(true);
                levelRequirementText.text = $"{data.requiredLevel}";
                priceIcon.SetActive(false);
            }
            else if (data.cost > 0)
            {
                priceIcon.SetActive(true);
                priceText.text = $"{data.cost}";
                levelIcon.SetActive(false);
            }         
            
        }
    }

        public void AssignMainButtonAction(System.Action<Item> action)
    {
        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(() => action?.Invoke(itemData));
    }

  
    public void AssignPriceButtonAction(System.Action<Item> action)
    {
        priceButton.onClick.RemoveAllListeners();
        priceButton.onClick.AddListener(() => action?.Invoke(itemData));
    }
}