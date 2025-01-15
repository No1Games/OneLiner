using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalSetup : MenuBase
{
    [SerializeField] private Button addPlayer;
    [SerializeField] private Button selectLeader;
    [SerializeField] private Button randomLeader;
    [SerializeField] private Button modeSelect;
    [SerializeField] private Button startGame;
    [SerializeField] private Button back;

    [SerializeField] private Animator ModeAnimator;
    [SerializeField] private GameObject ModeBGScreen;
    public override MenuName Menu => MenuName.LocalSetup;

    public event Action OnScreenShow;
    public event Action OnAddPlayerBtnClick;
    public event Action OnStartGameBtnClick;
    public event Action OnRandomLeaderBtnClick;
    public event Action OnChooseLeaderBtnClick;
    public event Action OnBackBtnClick;




    private void Awake()
    {
        Init();
    }

    
    public override void Init()
    {
        addPlayer.onClick.AddListener(OnClick_AddPlayerBtn);        
        selectLeader.onClick.AddListener(OnClick_SelectLeaderBtn);
        randomLeader.onClick.AddListener(OnClick_RandomLeaderBtn);
        
        startGame.onClick.AddListener(OnClick_StartGameBtn);
        back.onClick.AddListener(OnClick_Back);

    }
    

    public void AddPlayerBtn_VisibilityChange(bool visibility)
    {
        addPlayer.gameObject.SetActive(visibility);
        addPlayer.GetComponentInParent<LayoutElement>().ignoreLayout = !visibility;
    }

    public override void Show()
    {
        base.Show();
        OnScreenShow?.Invoke();
        
    }

    private void OnClick_AddPlayerBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        OnAddPlayerBtnClick?.Invoke();
        

    }
    
    private void OnClick_SelectLeaderBtn()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        OnChooseLeaderBtnClick?.Invoke();
    }
    private void OnClick_RandomLeaderBtn() 
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        OnRandomLeaderBtnClick?.Invoke();

    }

    
    private void OnClick_StartGameBtn()
    {
        OnStartGameBtnClick?.Invoke();
    }
    private void OnClick_Back()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        OnBackBtnClick?.Invoke();
        MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);
    }
}
