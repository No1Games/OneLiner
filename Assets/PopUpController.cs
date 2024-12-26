using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpController : MonoBehaviour
{
    [SerializeField] private GameObject mainElements;
    [SerializeField] private GameObject popUp_ApproveWithGems;
    [SerializeField] private GameObject popUp_ApproveWithNoGems;
    [SerializeField] private GameObject popUp_ApproveWithPremiumNoLvl;
    [SerializeField] private GameObject popUp_ApproveWithNoPremiumNoLvl;
    [SerializeField] private GameObject popUp_ApproveCombine;
    [SerializeField] private GameObject popUp_PurchaseSkinWithGems;
    [SerializeField] private GameObject popUp_PurchaseSkinWithNoGems;
    [SerializeField] private GameObject popUp_LeaveWithoutChanges;
    [SerializeField] private GameObject positiveElements;
    [SerializeField] private GameObject negativeElements;
    [SerializeField] private Button acceptBtn;
    [SerializeField] private Button cancelBtn;

    [SerializeField] private TextMeshProUGUI ApproveWithGemsPriceText;
    [SerializeField] private TextMeshProUGUI ApproveWithNoGemsPriceText;
    [SerializeField] private TextMeshProUGUI PurchaseSkinWithGemsPriceText;

    public event Action<List<Item>> OnPurchaseConfirmed;
    public event Action OnExitConfirmed;

    private List<GameObject> allPopUps;
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        allPopUps = new List<GameObject>();
        allPopUps.Add(popUp_ApproveWithGems);
        allPopUps.Add(popUp_ApproveWithNoGems);
        allPopUps.Add(popUp_ApproveWithPremiumNoLvl);
        allPopUps.Add(popUp_ApproveWithNoPremiumNoLvl);
        allPopUps.Add(popUp_ApproveCombine);
        allPopUps.Add(popUp_PurchaseSkinWithGems);
        allPopUps.Add(popUp_PurchaseSkinWithNoGems);
        allPopUps.Add(popUp_LeaveWithoutChanges);

        cancelBtn.onClick.AddListener(() =>
        {
            ClosePopUps();
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        });
    }

    private void ClosePopUps()
    {
        foreach (GameObject popUp in allPopUps)
        {
            popUp.SetActive(false);
        }
        mainElements.SetActive(false);

        acceptBtn.onClick.RemoveAllListeners();
        acceptBtn.onClick.AddListener(() => AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click));

    }

    private void OpenMainElements(bool IsPositive)
    {
        mainElements.SetActive(true);
        if (IsPositive)
        {
            positiveElements.SetActive(true);
            negativeElements.SetActive(false);

        }
        else
        {
            positiveElements.SetActive(false);
            negativeElements.SetActive(true);
        }

    }
    public void OpenPopUp_ApproveWithGems(List<Item> itemsToBuy) {

        OpenMainElements(true);
        popUp_ApproveWithGems.SetActive(true);

        acceptBtn.onClick.AddListener(() =>
        {
            
            OnPurchaseConfirmed?.Invoke(itemsToBuy);
            ClosePopUps();
        });
        int _itemPrice = 0;
        foreach (Item item in itemsToBuy)
        {
            _itemPrice += item.cost;
        }
        ApproveWithGemsPriceText.text = _itemPrice.ToString();
    }
    public void OpenPopUp_ApproveWithNoGems(int needGems)
    {
        OpenMainElements(false);
        popUp_ApproveWithNoGems.SetActive(true);
        acceptBtn.onClick.AddListener(() =>
        {
            
            MainMenuManager.Instance.OpenMenu(MenuName.GemShop);
            ClosePopUps();
        });
        ApproveWithNoGemsPriceText.text = needGems.ToString();

    }

    public void OpenPopUp_ApproveWithPremiumNoLvl()
    {
        OpenMainElements(false);
        popUp_ApproveWithPremiumNoLvl.SetActive(true);
        acceptBtn.onClick.AddListener(ClosePopUps);

    }

    public void OpenPopUp_ApproveWithNoPremiumNoLvl()
    {
        OpenMainElements(false);
        popUp_ApproveWithNoPremiumNoLvl.SetActive(true);
        acceptBtn.onClick.AddListener(() =>
        {
            MainMenuManager.Instance.OpenMenu(MenuName.PremiumShop);
            ClosePopUps();
        });


    }
    public void OpenPopUp_ApproveCombine()
    {
        OpenMainElements(false);
        popUp_ApproveCombine.SetActive(true);
        acceptBtn.onClick.AddListener(ClosePopUps);
    }

    public void OpenPopUp_PurchaseSkinWithGems(Item itemToBuy)
    {
        OpenMainElements(true);
        popUp_PurchaseSkinWithGems.SetActive(true);

        acceptBtn.onClick.AddListener(() =>
        {
            OnPurchaseConfirmed?.Invoke(new List<Item> { itemToBuy });
            ClosePopUps();
        });
        PurchaseSkinWithGemsPriceText.text = itemToBuy.cost.ToString();

    }


    public void OpenPopUp_PurchaseSkinWithNoGems()
    {
        OpenMainElements(false);
        popUp_PurchaseSkinWithNoGems.SetActive(true);
        acceptBtn.onClick.AddListener(ClosePopUps);
    }



    public void OpenPopUp_LeaveWithoutChanges()
    {
        OpenMainElements(true);
        popUp_LeaveWithoutChanges.SetActive(true);

        acceptBtn.onClick.AddListener(() =>
        {
            OnExitConfirmed?.Invoke();
            ClosePopUps();
        });
    }

   
}
