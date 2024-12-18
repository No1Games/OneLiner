using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    private static MainMenuManager _instance;
    public static MainMenuManager Instance => _instance;

    [SerializeField] private List<MenuBase> _menus;

    [Space]
    [SerializeField] private BGImageController _bgController;

    private void Awake()
    {
        _instance = this;
    }

    public void ChangeMenu(MenuName menu)
    {
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

    public void OpenMenu(MenuName menu)
    {
        _menus.Find(m => m.Menu == menu).Show();
    }

    public void OpenRoomPanel(bool isCreate)
    {
        _menus.Find(m => m.Menu == MenuName.RoomsList).Hide();
        (_menus.Find(m => m.Menu == MenuName.WaitingRoom) as WaitingRoomUI).Show(isCreate);
    }
}

public enum MenuName
{
    LocalOnline, RoomsList, WaitingRoom,
    MainScreen, LocalSetup, CustomizationScreen, ModeScreen, OptionScreen, MainShop, GemShop, PremiumShop
}
