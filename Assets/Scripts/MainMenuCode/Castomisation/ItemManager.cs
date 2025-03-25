
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Items")]
    [SerializeField] private List<Item> allItems; // �� ������ ����������� � ������ ������
    private Dictionary<int, Item> itemDictionary; // ��� �������� ������� �� �����

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void InitializeDictionary()
    {
        itemDictionary = allItems.ToDictionary(item => item.itemCode, item => item);
    }

    public Item GetItemByCode(int code)
    {
        
        if (itemDictionary.TryGetValue(code, out var item))
        {
            return item;
        }
        

        Debug.LogError($"Item with code {code} not found!");
        return itemDictionary[1001];
    }

    public List<int> GetItemCodesByCategory(ItemCategory category)
    {
        return allItems
            .Where(item => item.category == category)
            .Select(item => item.itemCode)
            .ToList();
    }
}


