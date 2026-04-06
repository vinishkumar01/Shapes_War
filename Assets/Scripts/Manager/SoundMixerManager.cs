using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;

    public static SoundMixerManager _instance;

    private Slider _masterVolumeSlider;
    private Slider _sfxVolumeSlider;
    private Slider _musicVolumeSlider;

    private string _currentScene;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

    }

    public void RegisterSlider(Slider masterVolume, Slider sfxVolume, Slider musicVolume)
    {
        _masterVolumeSlider = masterVolume;
        _sfxVolumeSlider = sfxVolume;
        _musicVolumeSlider = musicVolume;

        LoadVolume();
    }

    public void SetMasterVolume(float level)
    {
        level = Mathf.Clamp(level, 0.0001f, 1f);

        _audioMixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", level);
    }

    public void SetSFXVolume(float level)
    {
        level = Mathf.Clamp(level, 0.0001f, 1f);

        _audioMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", level);
    }

    public void SetMusicVolume(float level)
    {
        level = Mathf.Clamp(level, 0.0001f, 1f);

        _audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", level);
    }    

    private void LoadVolume()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);

        SetMasterVolume(master);
        SetSFXVolume(sfx);
        SetMusicVolume(music);

        if(_masterVolumeSlider != null)
        {
            _masterVolumeSlider.SetValueWithoutNotify(master);
        }
        
        if(_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.SetValueWithoutNotify(sfx);
        }
        
        if(_musicVolumeSlider != null)
        {
            _musicVolumeSlider.SetValueWithoutNotify(music);
        }
        
    }
}
