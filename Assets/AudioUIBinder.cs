using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioUIBinder : MonoBehaviour
{
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _sfxVolume;
    [SerializeField] private Slider _musicVolume;

    private void Start()
    {
        if (SoundMixerManager._instance == null)
        {
            Debug.LogError("SoundMixerManager not initialized");
            return;
        }

        SoundMixerManager._instance.RegisterSlider(_masterVolume, _sfxVolume, _musicVolume);
    }
}
