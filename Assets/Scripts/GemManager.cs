
using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance { get; private set; }

    private int gems; // Поточна кількість гемів 

    [SerializeField] private AccountManager accountManager;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadGemsFromAccount(); // Завантажуємо початкові дані
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Завантаження гемів із AccountData
    private void LoadGemsFromAccount()
    {
        gems = accountManager.GetAccountData().gems;
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
        accountManager.GetAccountData().gems = gems;
        accountManager.SaveAccount(); // Зберігаємо дані акаунта
    }
}

