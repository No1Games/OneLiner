using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainMenuManager : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    private static MainMenuManager _instance;
    public static MainMenuManager Instance => _instance;

    [SerializeField] private List<MenuBase> _menus;

    private void Awake()
    {
        _instance = this;
    }

    public void ChangeMenu(MenuName menu)
    {
        _logger.Log($"Change Menu to {menu}");

        foreach (var m in _menus)
        {
            if (m.Menu == menu)
            {
                m.Show();
            }
            else
            {
                m.Hide();
            }
        }
    }
}

public enum MenuName
{
    Auth, LobbyList, Lobby, LobbyCreate
}
