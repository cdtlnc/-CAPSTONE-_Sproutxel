using UnityEngine;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }

    [Header("Audio Mixer Reference")]
    [SerializeField] private AudioMixer audioMixer;

    // Properties used by your OptionsMenuUI script
    public float MasterVolume
    {
        get => PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        set { PlayerPrefs.SetFloat("MasterVolume", value); SetMixerVolume("MasterVol", value); }
    }

    // Change "MusicParam" to "MasterMusic"
    public float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        set { PlayerPrefs.SetFloat("MusicVolume", value); SetMixerVolume("MasterMusic", value); }
    }

    // Change "SFXParam" to "MasterSFX"
    public float SFXVolume
    {
        get => PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        set { PlayerPrefs.SetFloat("SFXVolume", value); SetMixerVolume("MasterSFX", value); }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load and apply saved settings on game startup
        SetMixerVolume("MasterParam", MasterVolume);
        SetMixerVolume("MusicParam", MusicVolume);
        SetMixerVolume("SFXParam", SFXVolume);
    }

    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (audioMixer == null) return;

        // Convert slider value (0 to 1) into decibels (-80dB to 20dB)
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}
