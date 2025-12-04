using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager Instance;
    [SerializeField] private AudioSource audioSourceMain;
    [SerializeField] private AudioSource audioSourceBack;
    [SerializeField] private AudioSource audioSourceAdditional;

    [SerializeField] AudioMixer audioMixer;
    [SerializeField] string musicVolumeParam;
    [SerializeField] string sfxVolumeParam;

    public const string MusicKey = "MusicVolume";
    public const string SFXKey = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager added to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("Duplicate AudioManager detected and destroyed");
            Destroy(gameObject);
        }

        PlaySoundInBack(GameSounds.MainTheme);
          
        
    }
    private void Start()
    {
        SetPreviousSettings();
    }

    private void SetPreviousSettings()
    {
        
        if (PlayerPrefs.HasKey(MusicKey))
        {
            float musicVolume = PlayerPrefs.GetFloat(MusicKey);
            Debug.Log($"Music Volume from PlayerPrefs: {musicVolume}");
            if (musicVolume <= -80f)
            {
                TurnMusicOff(); 
            }
            else
            {
                TurnMusicOn(); 
            }
        }
       

        
        if (PlayerPrefs.HasKey(SFXKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(SFXKey);
            Debug.Log($"SFX Volume from PlayerPrefs: {sfxVolume}");
            if (sfxVolume <= -80f)
            {
                TurnSFXOff(); 
            }
            else
            {
                TurnSFXOn(); 
            }
        }
       
    }


    public void PlaySoundInMain(GameSounds name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.Log($"Sound {name} not found");
            return;
        }
        audioSourceMain.clip = s.clip;
        audioSourceMain.loop = s.loop;
        audioSourceMain.Play();
    }

    public void PauseSoundInBack()
    {
        audioSourceBack.Stop();
    }

    public void ResumeSoundInBack()
    {
        audioSourceBack.Play();
    }

    public void PlaySoundInBack(GameSounds name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log($"Sound {name} not found");
            return;
        }
        audioSourceBack.clip = s.clip;
        audioSourceBack.loop = s.loop;
        audioSourceBack.Play();
    }

    public void PlaySoundInAdditional(GameSounds name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log($"Sound {name} not found");
            return;
        }
        audioSourceAdditional.clip = s.clip;
        audioSourceAdditional.loop = s.loop;
        audioSourceAdditional.Play();
    }

    public void StopSoundInAdditional()
    {
        audioSourceAdditional.Stop();
    }

    public void TurnMusicOn()
    {
        audioMixer.SetFloat(musicVolumeParam, 0f);  // Встановлення нормального рівня гучності
        PlayerPrefs.SetFloat(MusicKey, 0f);
        PlayerPrefs.Save();
    }

    public void TurnMusicOff()
    {
        audioMixer.SetFloat(musicVolumeParam, -80f);  // Встановлення мінімального рівня гучності (вимкнено)
        PlayerPrefs.SetFloat(MusicKey, -80f);
        PlayerPrefs.Save();
    }

    public void TurnSFXOn()
    {
        audioMixer.SetFloat(sfxVolumeParam, 0f);  // Встановлення нормального рівня гучності для SFX
        PlayerPrefs.SetFloat(SFXKey, 0f);
        PlayerPrefs.Save();
    }

    public void TurnSFXOff()
    {
        audioMixer.SetFloat(sfxVolumeParam, -80f);  // Встановлення мінімального рівня гучності для SFX (вимкнено)
        PlayerPrefs.SetFloat(SFXKey, -80f);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume()
    {
        audioMixer.GetFloat(musicVolumeParam, out float volume);
        return volume;
    }

    public float GetSFXVolume()
    {
        audioMixer.GetFloat(sfxVolumeParam, out float volume);
        return volume;
    }
}

public enum GameSounds
{
    MainTheme, Menu_Click, Menu_Play, Menu_edit,
    Game_WordMenu, Game_DrawMenu, Game_Settings, Game_TurnChange, Game_WrongWord, Game_WrongDrawing,
    Timer_LastSeconds, Game_Victory, Game_Lose
}
