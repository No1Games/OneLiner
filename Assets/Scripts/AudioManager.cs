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
    public void StopSoundInMain()
    {
        audioSourceMain.Stop();
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
        audioMixer.SetFloat(musicVolumeParam, 0f);
    }
    public void TurnMusicOff()
    {
        audioMixer.SetFloat(musicVolumeParam, -80f);
    }
    public void TurnSFXOn()
    {
        audioMixer.SetFloat(sfxVolumeParam, 0f);
    }
    public void TurnSFXOff()
    {
        audioMixer.SetFloat(sfxVolumeParam, -80f);
    }

    public float GetMusicVolume()
    {
        float volume;
        audioMixer.GetFloat(musicVolumeParam, out volume);
        return volume;
    }
    public float GetSFXVolume()
    {
        float volume;
        audioMixer.GetFloat(sfxVolumeParam, out volume);
        return volume;
    }


}

public enum GameSounds
{
MainTheme, Menu_Click, Menu_Play, Menu_edit,
Game_WordMenu, Game_DrawMenu, Game_Settings, Game_TurnChange, Game_WrongWord, Game_WrongDrawing, 
Timer_LastSeconds, Game_Victory, Game_Lose
}
