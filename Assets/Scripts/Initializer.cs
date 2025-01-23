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

        LoadingPanel.Instance.Hide();
    }

    public static async Task TryInitServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();

            (LobbyService.Instance as ILobbyServiceSDKConfiguration).EnableLocalPlayerLobbyEvents(true);
        }
    }

    public static async Task TrySignIn()
    {
        string serviceProfileName = $"player{Guid.NewGuid()}";

        var result = await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName.Substring(0, 30));

        Debug.Log($"Services Authentification Result: {result}");
    }
}
