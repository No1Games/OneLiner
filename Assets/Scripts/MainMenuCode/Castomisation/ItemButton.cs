using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button mainButton; // ������� ������
    [SerializeField] private Button priceButton; // ������ �������
    [SerializeField] private Image itemIcon; // ������ ������
    [SerializeField] private Image lockIcon; // �������
    [SerializeField] private GameObject levelIcon; // ������ ����
    [SerializeField] private GameObject priceIcon; // ������ ����
    [SerializeField] private TextMeshProUGUI priceText; // ֳ��
    [SerializeField] private TextMeshProUGUI levelRequirementText; // ������ �� ����

    [Header("State")]
    private Item itemData; // ��� ������, �� ������������ ������

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