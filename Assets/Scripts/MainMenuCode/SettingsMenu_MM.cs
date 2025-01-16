
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
        //Init(); // called from mainMenuManager
        
    }
    private void Start()
    {
        ButtonAnimationUpdate();
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
        CheckPlayerPrefs();




    }
    

    private void CheckPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(LocalSelector.LanguageKey))
        {
            int savedLanguageIndex = PlayerPrefs.GetInt(LocalSelector.LanguageKey);

            
            languageChange.value = savedLanguageIndex;

            
            languageChange.onValueChanged.Invoke(savedLanguageIndex);
        }
       

       
        
    }
    private void ButtonAnimationUpdate()
    {
       if (PlayerPrefs.HasKey(AudioManager.MusicKey))
        {
            float musicVolume = PlayerPrefs.GetFloat(AudioManager.MusicKey);
            
            if (musicVolume <= -80f)
            {
                musicBtnAnimator.SetTrigger("ClickOff");
            }
            else
            {
                musicBtnAnimator.SetTrigger("ClickOn");
            }
        }



        if (PlayerPrefs.HasKey(AudioManager.SFXKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(AudioManager.SFXKey);
            
            if (sfxVolume <= -80f)
            {
                sfxBtnAnimator.SetTrigger("ClickOff");
            }
            else
            {
                sfxBtnAnimator.SetTrigger("ClickOn");
            }
        }

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
