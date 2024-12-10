
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsMenu_MM : MenuBase
{
    [SerializeField] private Button musicVolumeChange;
    [SerializeField] private Button sfxVolumeChange;
    [SerializeField] private Button vibrationChange;
    [SerializeField] private Button messageUs;
    [SerializeField] private Button confidentialPolicy;
    [SerializeField] private Button back;
    [SerializeField] private TMP_Dropdown languageChange;

    [SerializeField] private Animator musicBtnAnimator;
    [SerializeField] private Animator sfxBtnAnimator;
    [SerializeField] private Animator vibrationBtnAnimator;

    [SerializeField] private LocalSelector localSelector;

    public override MenuName Menu => MenuName.OptionScreen;

    private void Awake()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        musicVolumeChange.onClick.AddListener(OnClick_musicVolumeChange);
        sfxVolumeChange.onClick.AddListener (OnClick_sfxVolumeChange);
        vibrationChange.onClick.AddListener(OnClick_vibrationChange);
        messageUs.onClick.AddListener(OnClick_messageUs);
        confidentialPolicy.onClick.AddListener(OnClick_confidentialPolicy);
        back.onClick.AddListener(OnClick_back);
        languageChange.onValueChanged.AddListener(OnLanguageChanged);

    }

    private void OnClick_musicVolumeChange()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if(AudioManager.Instance.GetMusicVolume() <= -80)
        {
            AudioManager.Instance.TurnMusicOn();
            musicBtnAnimator.SetTrigger("ClickOn");
        }
        else
        {
            AudioManager.Instance.TurnMusicOff();
            musicBtnAnimator.SetTrigger("ClickOff");
        }

    }
    private void OnClick_sfxVolumeChange()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (AudioManager.Instance.GetSFXVolume() <= -80)
        {
            AudioManager.Instance.TurnSFXOn();
            sfxBtnAnimator.SetTrigger("ClickOn");
        }
        else
        {
            AudioManager.Instance.TurnSFXOff();
            sfxBtnAnimator.SetTrigger("ClickOff");
        }
    }
        private void OnClick_vibrationChange()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);

    }
    private void OnClick_messageUs()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        localSelector.ChooseLocal(3);

    }
    private void OnClick_confidentialPolicy()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    }
    private void OnClick_back()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);
    }

    private void OnLanguageChanged(int index)
    {
        //index = languageChange.value;
        localSelector.ChooseLocal(index);
    }

}
