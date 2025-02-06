using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;  // Потрібно для Unity IAP

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


    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        gemFirstTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemSecondTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemThirdTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);
        gemMaxTierBtn.onPurchaseComplete.AddListener(GemPurchaseHandler);

        //subscriptionFirstTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseHandler);
        //subscriptionSecondTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseHandler);
        //subscriptionThirdTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseHandler);
        //subscriptionMaxTierBtn.onPurchaseComplete.AddListener(SubscriptionPurchaseHandler);

        //specialOfferBtn.onPurchaseComplete.AddListener(SpecialOfferHandler);
    }

    private void GemPurchaseHandler(Product product)
    {
        if (product.definition.id == gemFirstTierBtn.productId)
        {
            GemManager.Instance.AddGems(50);
        }
        else if (product.definition.id == gemSecondTierBtn.productId)
        {
            GemManager.Instance.AddGems(100);
        }
        else if (product.definition.id == gemThirdTierBtn.productId)
        {
            GemManager.Instance.AddGems(250);
        }
        else if (product.definition.id == gemMaxTierBtn.productId)
        {
            GemManager.Instance.AddGems(250);
        }

        else
        {
            Debug.LogWarning("Невідомий продукт: " + product.definition.id);
        }
    }

    private void SubscriptionPurchaseHandler(Product product)
    {
        if (product.definition.id == subscriptionFirstTierBtn.productId)
        {
            ActivateSubscription("subscription_1");
        }
        else if (product.definition.id == subscriptionSecondTierBtn.productId)
        {
            ActivateSubscription("subscription_2");
        }
        else if (product.definition.id == subscriptionThirdTierBtn.productId)
        {
            ActivateSubscription("subscription_3");
        }
        else if (product.definition.id == subscriptionMaxTierBtn.productId)
        {
            ActivateSubscription("subscription_max");
        }
        else
        {
            Debug.LogWarning("Невідома підписка: " + product.definition.id);
        }


    }

    private void ActivateSubscription(string duration)
    {

    }

    private void SpecialOfferHandler(Product product)
    {
        if (product.definition.id == "starter_pack")
        {
            //GetGems(1000);
            //UnlockSkin("StarterSkin");
            //ActivateSubscription();
        }
       
        else
        {
            Debug.LogWarning("Невідома спецпропозиція: " + product.definition.id);
        }
    }

}