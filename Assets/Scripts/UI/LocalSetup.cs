using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalSetup : MenuBase
{
    [SerializeField] private Button addPlayer;
    [SerializeField] private Button removePlayer;
    //[SerializeField] private Button editPlayer;
    [SerializeField] private Button selectLeader;
    [SerializeField] private Button randomLeader;
    [SerializeField] private Button modeSelect;
    [SerializeField] private Button startGame;
    [SerializeField] private Button back;
    public override MenuName Menu => MenuName.LocalSetup;

    private void Awake()
    {
        Init();
    }
    public override void Init()
    {
        addPlayer.onClick.AddListener(OnClick_AddPlayerBtn);
    }

    private void OnClick_AddPlayerBtn()
    {
        //AudioManager.Instance.PlaySound("MenuClick");
        //MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);

    }
}
