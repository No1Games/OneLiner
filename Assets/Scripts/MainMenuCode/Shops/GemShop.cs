using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemShop : MenuBase
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button freeTierBtn;
    [SerializeField] private Button firstTierBtn;
    [SerializeField] private Button secondTierBtn;
    [SerializeField] private Button thirdTierBtn;
    [SerializeField] private Button maxTierBtn;

    private MenuName previousMenu;
    public override MenuName Menu => MenuName.GemShop;

    private void Start()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        backBtn.onClick.AddListener(OnClick_BackButton);
        freeTierBtn.onClick.AddListener(() => GetGems(5));
        firstTierBtn.onClick.AddListener(() => GetGems(50));
        secondTierBtn.onClick.AddListener(() => GetGems(300));
        thirdTierBtn.onClick.AddListener(() => GetGems(1000));
        maxTierBtn.onClick.AddListener(() => GetGems(2500));

    }

    private void GetGems(int gems)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        GemManager.Instance.AddGems(gems);
        Debug.Log("Total gems now " + GemManager.Instance.GetGems());
    }

    public void SetPreviousMenu(MenuName menu)
    {
        previousMenu = menu;
    }

    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(previousMenu);
    }


}
