
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
        AccountManager.Instance.OnAccountInitializationComplete += LoadGemsFromAccount; // ����������� �������� ���
        
    }

    // ������������ ���� �� AccountData
    private void LoadGemsFromAccount()
    {
        gems =  AccountManager.Instance.CurrentAccountData.gems;
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
        AccountManager.Instance.CurrentAccountData.gems = gems;
        AccountManager.Instance.SaveAccountData(); // �������� ��� �������
    }
}

