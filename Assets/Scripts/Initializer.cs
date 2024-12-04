using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    private async void Awake()
    {
        LoadingPanel.Instance.Show("Loading...");

        await InitializeServices();

        LoadingPanel.Instance.Hide();
    }

    public async Task InitializeServices()
    {
        string serviceProfileName = $"player{Guid.NewGuid()}";

        var result = await UnityServiceAuthenticator.TrySignInAsync(serviceProfileName.Substring(0, 30));

        (LobbyService.Instance as ILobbyServiceSDKConfiguration).EnableLocalPlayerLobbyEvents(true);

        Debug.Log($"Services Authentification Result: {result}");
    }
}
