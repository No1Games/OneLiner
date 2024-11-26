
using UnityEngine;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance { get; private set; }

    private int gems; // ������� ������� ���� 

    [SerializeField] private AccountManager accountManager;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadGemsFromAccount(); // ����������� �������� ���
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ������������ ���� �� AccountData
    private void LoadGemsFromAccount()
    {
        gems = accountManager.GetAccountData().gems;
    }

    // ��������� ����
    public void AddGems(int amount)
    {
        gems += amount;
        UpdateAccountData();
    }

    // �������� ����
    public void SpendGems(int amount)
    {
        if (gems >= amount)
        {
            gems -= amount;
            UpdateAccountData();
        }
        
    }

    // ��������� ������� ������� ����
    public int GetGems()
    {
        return gems;
    }

    // ��������� ���� � AccountData
    private void UpdateAccountData()
    {
        accountManager.GetAccountData().gems = gems;
        accountManager.SaveAccount(); // �������� ��� �������
    }
}

