using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    private static MainMenuManager _instance;
    public static MainMenuManager Instance => _instance;

    [SerializeField] private List<MenuBase> _menus;

    private MenuName _currentMenu;
    private Stack<MenuName> _stackMenus = new();

    [Space]
    [SerializeField] private BGImageController _bgController;

    private void Awake()
    {
        _instance = this;
        _currentMenu = MenuName.MainScreen;
        CallSettingsInit();
    }

    private void Start()
    {
        if (IngameData.Instance.ReturnedFromGame)
        {
            ChangeMenu(MenuName.LocalSetup);
        }
    }

    private void CallSettingsInit()
    {
        // Знаходимо об'єкт з MenuName.OptionScreen
        var settingsMenu = _menus.Find(menu => menu.Menu == MenuName.OptionScreen);

        // Перевіряємо, чи знайдено меню
        if (settingsMenu != null)
        {
            settingsMenu.Init();
        }
        else
        {
            Debug.LogWarning("Menu with MenuName.OptionScreen not found in _menus list!");
        }
    }

    public void ChangeMenu(MenuName menu)
    {
        _stackMenus.Push(_currentMenu);
        _currentMenu = menu;
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

        // Special Case: Local Online menu opens on top of main screen.
        if (menu == MenuName.LocalOnline)
        {
            OpenMenu(MenuName.MainScreen);
        }

        // Set Background Color here
        if (menu == MenuName.RoomsList)
        {
            _bgController.SetBackground(BGType.Blue);
        }
        else
        {
            _bgController.SetBackground(BGType.White);
        }
    }

    public void ChangeMenuToPrevious()
    {
        ChangeMenu(_stackMenus.Peek());
    }

    public void OpenMenu(MenuName menu)
    {
        _stackMenus.Push(_currentMenu);
        _currentMenu = menu;
        _menus.Find(m => m.Menu == menu).Show();
    }

    public void OpenRoomPanel(bool isCreate)
    {
        MenuBase roomListMenu = _menus.Find(m => m.Menu == MenuName.RoomsList);

        if (roomListMenu != null && roomListMenu.isActiveAndEnabled)
        {
            if (roomListMenu.isActiveAndEnabled)
            {
                roomListMenu.Hide();
            }
            else
            {
                Debug.Log("Room List Menu is already inactive");
            }
        }
        else
        {
            Debug.LogWarning("Room List menu is null.");
        }

        WaitingRoomUI waitingRoomMenu = _menus.Find(m => m.Menu == MenuName.WaitingRoom) as WaitingRoomUI;

        if (waitingRoomMenu == null)
        {
            Debug.LogWarning("Waiting menu is null! Leaving lobby.");
            OnlineController.Instance.LeaveLobby();
            return;
        }

        waitingRoomMenu.Show(isCreate);
    }
}

public enum MenuName
{
    LocalOnline, RoomsList, WaitingRoom,
    MainScreen, LocalSetup, CustomizationScreen, ModeScreen, OptionScreen, MainShop, GemShop, PremiumShop
}
