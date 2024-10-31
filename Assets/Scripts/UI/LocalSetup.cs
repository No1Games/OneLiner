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
        removePlayer.onClick.AddListener (OnClick_RemovePlayerBtn);
        selectLeader.onClick.AddListener(OnClick_SelectLeaderBtn);
        randomLeader.onClick.AddListener(OnClick_RandomLeaderBtn);
        modeSelect.onClick.AddListener(OnClick_ModeSelectBtn);
        startGame.onClick.AddListener(OnClick_StartGameBtn);
        back.onClick.AddListener(OnClick_Back);

    }

    private void OnClick_AddPlayerBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        //MainMenuManager.Instance.OpenMenu(MenuName.LocalOnline);

    }
    private void OnClick_RemovePlayerBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_SelectLeaderBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_RandomLeaderBtn() 
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }

    private void OnClick_ModeSelectBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_StartGameBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Play);
    }
    private void OnClick_Back()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
}
