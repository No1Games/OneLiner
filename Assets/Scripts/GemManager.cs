
using System;
using UnityEngine;
using System.Threading.Tasks;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance { get; private set; }
    

    private int gems;     

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        AccountManager.Instance.OnAccountInitializationComplete += LoadGemsFromAccount; // Завантажуємо початкові дані
        
    }

    // Завантаження гемів із AccountData
    private void LoadGemsFromAccount()
    {
        gems =  AccountManager.Instance.CurrentAccountData.gems;
    }

    // Додавання гемів
    public void AddGems(int amount)
    {
        gems += amount;
        UpdateAccountData();
    }

    // Списання гемів
    public void SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            UpdateAccountData();
        }
        
    }

    // Отримання поточної кількості гемів
    public int GetGems()
    {
        return gems;
    }

    // Оновлення гемів у AccountData
    private void UpdateAccountData()
    {
        AccountManager.Instance.CurrentAccountData.gems = gems;
        AccountManager.Instance.SaveAccountData(); // Зберігаємо дані акаунта
    }
}

