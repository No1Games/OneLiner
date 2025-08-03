using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    private async void Awake()
    {

        LoadingPanel.Instance.Show();

        await TryInitServices();

        await TrySignIn();

        bool isGoogleSignedIn = await TryGooglePlaySignIn();

        await AccountManager.Instance.InitializeAccountAsync(isGoogleSignedIn);

        LoadingPanel.Instance.Hide();
    }

    public static async Task TryInitServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();

            (LobbyService.Instance as ILobbyServiceSDKConfiguration).EnableLocalPlayerLobbyEvents(false);
        }
    }

    public static async Task TrySignIn()
    {
        string serviceProfileName = $"player{Guid.NewGuid()}";

        var result = await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName.Substring(0, 30));

        Debug.Log($"Services Authentification Result: {result}");
    }

    public static async Task<bool> TryGooglePlaySignIn()
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("Google Play Sign-in successful");
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogWarning($"Google Play Sign-in failed: {status}");
                tcs.SetResult(false);
            }
        });

        return await tcs.Task;
    }
}
