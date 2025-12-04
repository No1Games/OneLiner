// SubscriptionManager.cs
using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SubscriptionManager 
{
    private const string TimeApiUrl = "https://timeapi.io/api/Time/current/zone?timeZone=UTC";

    public async Task CheckSubscription(AccountData accountData)
    {
        if (accountData.hasLifetimeSubscription)
        {
            Debug.Log("Lifetime subscription active. No further checks needed.");
            return;
        }

        if (accountData.accountStatus != AccountStatus.Premium)
        {
            Debug.Log("User is not premium. No subscription to check.");
            return;
        }

        DateTime currentTime;
        try
        {
            currentTime = await GetCurrentTimeUtc();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve current time: " + ex.Message);
            return;
        }

        if (accountData.subscriptionEndDate > currentTime)
        {
            Debug.Log("Subscription is still active.");
            return;
        }

        Debug.Log("Subscription expired. Downgrading user.");
        accountData.accountStatus = AccountStatus.Basic;
        AccountManager.Instance.SaveAccountData();
    }

    public async Task AddSubscriptionMonths(int monthsToAdd)
    {
        var accountData = AccountManager.Instance.CurrentAccountData;

        DateTime currentTime;
        try
        {
            currentTime = await GetCurrentTimeUtc();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve current time: " + ex.Message);
            return;
        }

        DateTime baseDate = accountData.accountStatus == AccountStatus.Premium
            ? accountData.subscriptionEndDate
            : currentTime;

        accountData.subscriptionEndDate = baseDate.AddMonths(monthsToAdd);
        accountData.accountStatus = AccountStatus.Premium;
        AccountManager.Instance.SaveAccountData();
    }
    public async Task AddSubscriptionDays(int daysToAdd)
    {
        var accountData = AccountManager.Instance.CurrentAccountData;

        DateTime currentTime;
        try
        {
            currentTime = await GetCurrentTimeUtc();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to retrieve current time: " + ex.Message);
            return;
        }

        DateTime baseDate = accountData.accountStatus == AccountStatus.Premium
            ? accountData.subscriptionEndDate
            : currentTime;

        accountData.subscriptionEndDate = baseDate.AddDays(daysToAdd);
        accountData.accountStatus = AccountStatus.Premium;
        AccountManager.Instance.SaveAccountData();
    }

    private async Task<DateTime> GetCurrentTimeUtc()
    {
        using UnityWebRequest request = UnityWebRequest.Get(TimeApiUrl);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new Exception("Time API request failed: " + request.error);
        }

        string json = request.downloadHandler.text;
        TimeApiResponse response = JsonUtility.FromJson<TimeApiResponse>(json);

        if (!DateTime.TryParseExact(response.dateTime, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedTime))
        {
            throw new Exception("Failed to parse time string: " + response.dateTime);
        }

        return parsedTime;
    }

    [Serializable]
    private class TimeApiResponse
    {
        public string dateTime;
    }
}
