using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;  // Потрібно для Unity IAP
using System.Threading.Tasks;

public class IAPManager : MonoBehaviour
{
    [SerializeField] private CodelessIAPButton gemFirstTierBtn;
    [SerializeField] private CodelessIAPButton gemSecondTierBtn;
    [SerializeField] private CodelessIAPButton gemThirdTierBtn;
    [SerializeField] private CodelessIAPButton gemMaxTierBtn;

    [SerializeField] private CodelessIAPButton subscriptionFirstTierBtn;
    [SerializeField] private CodelessIAPButton subscriptionSecondTierBtn;
    [SerializeField] private CodelessIAPButton subscriptionThirdTierBtn;
    [SerializeField] private CodelessIAPButton subscriptionMaxTierBtn;

    [SerializeField] private CodelessIAPButton specialOfferBtn;
    [SerializeField] private List<Item> specialOfferItems;
    private SubscriptionManager subscriptionManager;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        subscriptionManager = new();

        gemFirstTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemSecondTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemThirdTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemMaxTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);

        subscriptionFirstTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseProxy);
        subscriptionSecondTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseProxy);
        subscriptionThirdTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseProxy);
        subscriptionMaxTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseProxy);

        specialOfferBtn.onPurchaseComplete.AddListener(SpecialOfferPurchaseProxy);
    }

    private void GemPurchaseHandler(Product product)
    {
        if (product.definition.id == gemFirstTierBtn.productId)
        {
            GemManager.Instance.AddGems(50);
        }
        else if (product.definition.id == gemSecondTierBtn.productId)
        {
            GemManager.Instance.AddGems(300);
        }
        else if (product.definition.id == gemThirdTierBtn.productId)
        {
            GemManager.Instance.AddGems(1100);
        }
        else if (product.definition.id == gemMaxTierBtn.productId)
        {
            GemManager.Instance.AddGems(3000);
        }

        else
        {
            Debug.LogWarning("Невідомий продукт: " + product.definition.id);
        }
    }
    private void SubscriptionPurchaseProxy(Product product)
    {
        _ = SubscriptionPurchaseHandlerAsync(product);
    }
    private async Task SubscriptionPurchaseHandlerAsync(Product product)
    {
        if (product.definition.id == subscriptionFirstTierBtn.productId)
        {
            await subscriptionManager.AddSubscriptionMonths(1);
        }
        else if (product.definition.id == subscriptionSecondTierBtn.productId)
        {
            await subscriptionManager.AddSubscriptionMonths(3);
        }
        else if (product.definition.id == subscriptionThirdTierBtn.productId)
        {
            await subscriptionManager.AddSubscriptionMonths(6);
        }
        else if (product.definition.id == subscriptionMaxTierBtn.productId)
        {
            var data = AccountManager.Instance.CurrentAccountData;
            data.accountStatus = AccountStatus.Premium;
            data.hasLifetimeSubscription = true;
            AccountManager.Instance.SaveAccountData();
        }
        else
        {
            Debug.LogWarning("Невідома підписка: " + product.definition.id);
        }
    }



    private void SpecialOfferPurchaseProxy(Product product)
    {
        _ = SpecialOfferPurchaseHandlerAsync(product);
    }

    // make proxy method and async method
    private async Task SpecialOfferPurchaseHandlerAsync(Product product)
    {
        if (product.definition.id == "starter_pack")
        {
            GemManager.Instance.AddGems(500);
            await subscriptionManager.AddSubscriptionDays(7);
            AccountManager.Instance.AddItemsToAccount(specialOfferItems);
            AccountManager.Instance.SaveAccountData();

        }
       
        else
        {
            Debug.LogWarning("Невідома спецпропозиція: " + product.definition.id);
        }
    }

}